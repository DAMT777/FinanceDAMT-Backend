using FinanceDAMT.Application.Features.Transactions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Queries.GetTransactionById;

public sealed record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionDto>;
