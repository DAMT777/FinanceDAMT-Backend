using FinanceDAMT.Application.Features.AI.DTOs;

namespace FinanceDAMT.Application.Common.Interfaces;

public interface IAIService
{
    Task<string> ParseNaturalLanguageExpense(string input);
    Task<ChatResponseDto> ChatAsync(Guid userId, string userMessage, List<ChatMessageDto> history);
    Task<List<string>> GenerateRecommendations(Guid userId);
    Task<MonthlySummaryDto> GenerateMonthlySummary(Guid userId, int month, int year);
    Task<FinancialScoreDto> CalculateFinancialScore(Guid userId);
    Task<SpendingPredictionDto> PredictNextMonth(Guid userId);
}
