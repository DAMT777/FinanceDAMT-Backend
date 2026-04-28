using FinanceDAMT.Application.Features.Transactions.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Commands.CreateRecurringTransaction;

public sealed record CreateRecurringTransactionCommand(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    string? Description
) : IRequest<TransactionDto>;
