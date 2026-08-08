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

        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
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
        const string systemPrompt =
            "You are the personal finance assistant for the FinanceDAMT app. " +
            "Answer using ONLY the data in Context. The Context is a full snapshot of the user's " +
            "finances in this app and includes: currentDate; accounts and netWorth; expense totals by " +
            "category; recentTransactions (individual movements WITH their dates, so you CAN answer " +
            "about a specific day, this week, or a specific expense); savings goals; budgets; active " +
            "subscriptions; ventures (entrepreneurship projects with investment, units produced/sold, " +
            "revenue, net balance and ROI %); and monthly balances. " +
            "You are able to answer ANY question about the user's data in this app: net worth, account " +
            "balances, income vs expenses, savings rate and projections, weekly or daily spending (use " +
            "recentTransactions and their dates together with currentDate), budgets, savings goals, " +
            "subscriptions, and ventures/ROI. If the user asks about ROI or a venture, use the ventures " +
            "data to explain the concept AND give their real figures (invested, revenue, net balance, " +
            "ROI %). Do NOT say you only have access to a limited subset of data. " +
            "If a specific figure genuinely isn't in the Context, say so briefly and offer the closest " +
            "available insight — do not pad answers with unrelated data such as subscriptions unless asked. " +
            "Amounts are in the user's local currency (Colombian pesos, COP). " +
            "To assign money to a savings goal, the user must phrase it as an action such as " +
            "\"asigna el 10% de mi balance a mi meta del viaje\" or \"aporta 50000 a mi meta\"; " +
            "the app records that for real. Do not claim you assigned or moved money yourself. " +
            "Reply in the same language the user writes in. Keep responses practical and concise.";
        var response = await CompleteChatAsync(_smartModel, systemPrompt, userPrompt);

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
        var now = DateTime.UtcNow;
        var since = now.Date.AddMonths(-6);

        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => new { a.Name, Type = a.Type.ToString(), a.Balance })
            .ToListAsync();
        var netWorth = accounts.Sum(a => a.Balance);

        var totalsByCategory = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= since)
            .GroupBy(t => t.Category.Name)
            .Select(g => new { Category = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        // Individual recent movements (with dates) so the assistant can answer
        // day/week-specific questions, not just monthly aggregates.
        var recentSince = now.Date.AddDays(-45);
        var recentTransactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= recentSince)
            .OrderByDescending(t => t.Date)
            .Take(80)
            .Select(t => new
            {
                Date = t.Date.ToString("yyyy-MM-dd"),
                Type = t.Type.ToString(),
                Category = t.Category.Name,
                Account = t.Account.Name,
                t.Amount,
                t.Description
            })
            .ToListAsync();

        var goals = await _context.SavingGoals
            .Where(g => g.UserId == userId)
            .Select(g => new { g.Name, g.TargetAmount, g.CurrentAmount, g.Deadline })
            .ToListAsync();

        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId && b.Month == now.Month && b.Year == now.Year)
            .Select(b => new { Category = b.Category.Name, b.MonthlyLimit })
            .ToListAsync();

        var subsRaw = await _context.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .Select(s => new { s.Name, s.Amount, s.BillingCycle, s.NextBillingDate })
            .ToListAsync();

        var subscriptions = subsRaw
            .Select(s => new
            {
                s.Name,
                s.Amount,
                BillingCycle = s.BillingCycle.ToString(),
                NextBillingDate = s.NextBillingDate.ToString("yyyy-MM-dd"),
                MonthlyEquivalent = Math.Round(ToMonthlyAmount(s.Amount, s.BillingCycle), 2)
            })
            .ToList();

        var subscriptionsMonthlyTotal = Math.Round(subscriptions.Sum(s => s.MonthlyEquivalent), 2);

        // Ventures (entrepreneurship / ROI): aggregate each project's batches.
        var ventureEntities = await _context.Ventures
            .Where(v => v.UserId == userId)
            .Include(v => v.Batches)
            .ToListAsync();

        var ventures = ventureEntities.Select(v =>
        {
            var batches = v.Batches.Where(b => !b.IsDeleted).ToList();
            var investment = batches.Sum(b => b.Investment);
            var unitsProduced = batches.Sum(b => b.UnitsProduced);
            var unitsSold = batches.Sum(b => b.UnitsSold);
            var revenue = Math.Round(batches.Sum(b => b.UnitsSold * b.UnitPrice), 2);
            var netBalance = revenue - investment;
            var roiPercent = investment > 0 ? Math.Round((netBalance / investment) * 100m, 2) : 0m;
            return new
            {
                v.Name,
                v.IsActive,
                Investment = investment,
                UnitsProduced = unitsProduced,
                UnitsSold = unitsSold,
                UnitsRemaining = Math.Max(0, unitsProduced - unitsSold),
                Revenue = revenue,
                NetBalance = netBalance,
                RoiPercent = roiPercent,
                BatchCount = batches.Count
            };
        }).ToList();

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
            currentDate = now.ToString("yyyy-MM-dd"),
            accounts,
            netWorth,
            totalsByCategory,
            recentTransactions,
            goals,
            budgets,
            subscriptions,
            subscriptionsMonthlyTotal,
            ventures,
            monthlyBalance = monthlyBalance.Select(x => new { x.Year, x.Month, Balance = x.Income - x.Expenses })
        });
    }

    private static decimal ToMonthlyAmount(decimal amount, BillingCycle cycle) => cycle switch
    {
        BillingCycle.Weekly => amount * 52m / 12m,
        BillingCycle.Monthly => amount,
        BillingCycle.Quarterly => amount / 3m,
        BillingCycle.Yearly => amount / 12m,
        _ => amount
    };

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
                    return _httpClient.PostAsync("chat/completions", content);
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
