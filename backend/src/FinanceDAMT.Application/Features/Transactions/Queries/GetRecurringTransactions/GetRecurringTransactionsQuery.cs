using FinanceDAMT.Application.Features.Transactions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Queries.GetRecurringTransactions;

public sealed record GetRecurringTransactionsQuery : IRequest<IReadOnlyList<TransactionDto>>;
