using FinanceDAMT.Application.Features.Transactions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Queries.GetTransactions;

public sealed record GetTransactionsQuery(TransactionFilterRequest Filter) : IRequest<PaginatedResult<TransactionDto>>;
