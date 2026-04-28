using FinanceDAMT.Application.Features.Transactions.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Commands.CreateTransaction;

public sealed record CreateTransactionCommand(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    string? Description
) : IRequest<TransactionDto>;
