using FinanceDAMT.Application.Features.Transactions.DTOs;

namespace FinanceDAMT.Application.Features.AI.DTOs;

public sealed record AgentLogResultDto(bool Stored, string Message, TransactionDto? Transaction);
