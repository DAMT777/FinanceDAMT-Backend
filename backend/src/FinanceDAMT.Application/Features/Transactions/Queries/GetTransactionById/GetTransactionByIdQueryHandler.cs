using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Transactions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Queries.GetTransactionById;

public sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly ITransactionRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionByIdQueryHandler(ITransactionRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var transaction = await _repository.GetByIdAsync(userId, request.Id, cancellationToken)
            ?? throw new NotFoundException("Transaction not found.");

        return transaction;
    }
}
