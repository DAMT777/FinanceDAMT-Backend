using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Transactions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Queries.GetRecurringTransactions;

public sealed class GetRecurringTransactionsQueryHandler : IRequestHandler<GetRecurringTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    private readonly ITransactionRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetRecurringTransactionsQueryHandler(ITransactionRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TransactionDto>> Handle(GetRecurringTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        return await _repository.GetRecurringAsync(userId, cancellationToken);
    }
}
