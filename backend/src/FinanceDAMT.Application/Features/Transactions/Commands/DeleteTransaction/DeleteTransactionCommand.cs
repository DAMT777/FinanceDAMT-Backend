using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Commands.DeleteTransaction;

public sealed record DeleteTransactionCommand(Guid Id) : IRequest<Unit>;
