using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Domain.Enums;
using FinanceDAMT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace FinanceDAMT.Infrastructure.Services;

public sealed class GroqAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GroqAIService> _logger;
    private readonly string _smartModel;
    private readonly string _fastModel;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    public GroqAIService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context, ILogger<GroqAIService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;

        var apiKey = configuration["Groq:ApiKey"] ?? string.Empty;
        var baseUrl = configuration["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1";
        _smartModel = configuration["Groq:SmartModel"] ?? "llama-3.3-70b-versatile";
        _fastModel = configuration["Groq:FastModel"] ?? "llama-3.1-8b-instant";

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, retryCount, _) =>
                {
                    _logger.LogWarning("Groq retry {RetryCount} after {Delay}s. Status: {StatusCode}", retryCount, timespan.TotalSeconds, outcome.Result?.StatusCode);
                });

        _circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }

    public async Task<string> ParseNaturalLanguageExpense(string input)
    {
        var prompt = "Extract amount and category from the user text. Return only JSON with this exact shape: {\"amount\": 15000, \"categoryName\": \"Food\"}.";
        var raw = await CompleteChatAsync(_fastModel, prompt, input);
        return raw;
    }

    public async Task<ChatResponseDto> ChatAsync(Guid userId, string userMessage, List<ChatMessageDto> history)
    {
        var contextPrompt = await BuildFinancialContextPrompt(userId);
        var historyPrompt = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));
        var userPrompt = $"Context:\n{contextPrompt}\n\nConversation:\n{historyPrompt}\n\nUser: {userMessage}";
        var response = await CompleteChatAsync(_smartModel, "You are a personal finance assistant. Keep responses practical and concise.", userPrompt);

        var updatedHistory = history.ToList();
        updatedHistory.Add(new ChatMessageDto("user", userMessage, DateTime.UtcNow));
        updatedHistory.Add(new ChatMessageDto("assistant", response, DateTime.UtcNow));

        return new ChatResponseDto(response, updatedHistory);
    }

    public async Task<List<string>> GenerateRecommendations(Guid userId)
    {
        var contextPrompt = await BuildFinancialContextPrompt(userId);
        var response = await CompleteChatAsync(
            _smartModel,
            "Generate 3 to 5 actionable personal finance recommendations. Return only JSON array of strings.",
            contextPrompt);

        return TryParseStringArray(response) ?? [
            "Track weekly expenses by category to reduce budget drift.",
            "Set a fixed transfer to savings right after each income event.",
            "Review recurring subscriptions and cancel unused services."
        ];
    }

    public async Task<MonthlySummaryDto> GenerateMonthlySummary(Guid userId, int month, int year)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var previousStart = monthStart.AddMonths(-1);
        var previousEnd = monthStart;

        var income = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income && t.Date >= monthStart && t.Date < monthEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var expenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= monthStart && t.Date < monthEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var prevExpenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= previousStart && t.Date < previousEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var topCategory = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= monthStart && t.Date < monthEnd)
            .GroupBy(t => t.Category.Name)
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .Select(g => g.Key)
            .FirstOrDefaultAsync() ?? "N/A";

        var change = prevExpenses == 0m ? 0m : Math.Round(((expenses - prevExpenses) / prevExpenses) * 100m, 2);

        var narrative = await CompleteChatAsync(
            _smartModel,
            "Generate a concise monthly finance summary in plain text.",
            $"Income: {income}; Expenses: {expenses}; TopCategory: {topCategory}; PreviousMonthExpenseDeltaPct: {change}");

        return new MonthlySummaryDto(income, expenses, topCategory, change, [], narrative);
    }

    public async Task<FinancialScoreDto> CalculateFinancialScore(Guid userId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var income = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income && t.Date >= monthStart && t.Date < monthEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var expenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= monthStart && t.Date < monthEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var savingsRatio = income <= 0 ? 0m : Math.Max(0m, (income - expenses) / income);
        var savingsPoints = (int)Math.Round(Math.Min(25m, savingsRatio * 25m));

        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId && b.Month == now.Month && b.Year == now.Year)
            .ToListAsync();
        var budgetPoints = budgets.Count == 0 ? 10 : budgets.Count(b => !b.AlertSent100) * 25 / budgets.Count;

        var goals = await _context.SavingGoals
            .Where(g => g.UserId == userId)
            .ToListAsync();
        var goalProgressAvg = goals.Count == 0 ? 0m : goals.Average(g => g.TargetAmount <= 0 ? 0 : Math.Min(1m, g.CurrentAmount / g.TargetAmount));
        var goalPoints = (int)Math.Round(goalProgressAvg * 25m);

        var distinctCategories = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= monthStart && t.Date < monthEnd)
            .Select(t => t.CategoryId)
            .Distinct()
            .CountAsync();
        var diversificationPoints = Math.Min(25, distinctCategories * 3);

        var score = Math.Clamp(savingsPoints + budgetPoints + goalPoints + diversificationPoints, 0, 100);

        return new FinancialScoreDto(
            score,
            savingsPoints,
            budgetPoints,
            goalPoints,
            diversificationPoints,
            "Score computed from savings ratio, budget compliance, goal progress, and expense diversification.");
    }

    public async Task<SpendingPredictionDto> PredictNextMonth(Guid userId)
    {
        var start = DateTime.UtcNow.Date.AddMonths(-3);

        var byCategory = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= start)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategoryPredictionDto(g.Key, Math.Round(g.Sum(x => x.Amount) / 3m, 2)))
            .OrderByDescending(x => x.Amount)
            .ToListAsync();

        var total = byCategory.Sum(x => x.Amount);
        var narrative = await CompleteChatAsync(
            _smartModel,
            "Generate a short spending forecast explanation.",
            $"PredictedTotal: {total}; Categories: {string.Join(", ", byCategory.Select(c => c.CategoryName + "=" + c.Amount))}");

        return new SpendingPredictionDto(total, byCategory, narrative);
    }

    private async Task<string> BuildFinancialContextPrompt(Guid userId)
    {
        var since = DateTime.UtcNow.Date.AddMonths(-6);

        var totalsByCategory = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= since)
            .GroupBy(t => t.Category.Name)
            .Select(g => new { Category = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var goals = await _context.SavingGoals
            .Where(g => g.UserId == userId)
            .Select(g => new { g.Name, g.TargetAmount, g.CurrentAmount })
            .ToListAsync();

        var monthlyBalance = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= since)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                Expenses = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return JsonSerializer.Serialize(new
        {
            totalsByCategory,
            goals,
            monthlyBalance = monthlyBalance.Select(x => new { x.Year, x.Month, Balance = x.Income - x.Expenses })
        });
    }

    private async Task<string> CompleteChatAsync(string model, string systemPrompt, string userPrompt)
    {
        var requestBody = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2
        };

        var serialized = JsonSerializer.Serialize(requestBody);

        try
        {
            var response = await _retryPolicy.WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(() =>
                {
                    var content = new StringContent(serialized, Encoding.UTF8, "application/json");
                    return _httpClient.PostAsync("/chat/completions", content);
                });

            var payload = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Groq call failed with status {Status}: {Payload}", response.StatusCode, payload);
                return "I could not process the request right now. Please try again shortly.";
            }

            using var doc = JsonDocument.Parse(payload);
            var message = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return message ?? string.Empty;
        }
        catch (BrokenCircuitException)
        {
            return "AI service is temporarily unavailable due to upstream instability. Please try again in a few moments.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Groq call failed unexpectedly.");
            return "I could not process the request right now. Please try again shortly.";
        }
    }

    private static List<string>? TryParseStringArray(string raw)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(raw);
        }
        catch
        {
            return null;
        }
    }
}
