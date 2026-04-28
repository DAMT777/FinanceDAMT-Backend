using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Accounts.Commands.TransferBetweenAccounts;

public sealed class TransferBetweenAccountsCommandHandler : IRequestHandler<TransferBetweenAccountsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public TransferBetweenAccountsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.FromAccountId, cancellationToken)
            ?? throw new NotFoundException("Source account not found.");

        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.ToAccountId, cancellationToken)
            ?? throw new NotFoundException("Target account not found.");

        if (fromAccount.UserId != userId || toAccount.UserId != userId)
            throw new UnauthorizedException("Both accounts must belong to current user.");

        if (fromAccount.Balance < request.Amount)
            throw new ConflictException("Insufficient balance in source account.");

        fromAccount.Balance -= request.Amount;
        toAccount.Balance += request.Amount;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
