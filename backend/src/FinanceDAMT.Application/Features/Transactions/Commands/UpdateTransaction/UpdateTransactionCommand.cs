using FinanceDAMT.Application.Features.Transactions.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Commands.UpdateTransaction;

public sealed record UpdateTransactionCommand(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    string? Description,
    bool IsRecurring
) : IRequest<TransactionDto>;
