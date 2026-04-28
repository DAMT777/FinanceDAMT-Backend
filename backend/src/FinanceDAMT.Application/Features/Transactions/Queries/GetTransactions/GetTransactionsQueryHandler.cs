using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Transactions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Queries.GetTransactions;

public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedResult<TransactionDto>>
{
    private readonly ITransactionRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionsQueryHandler(ITransactionRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        return await _repository.GetTransactionsAsync(userId, request.Filter, cancellationToken);
    }
}
