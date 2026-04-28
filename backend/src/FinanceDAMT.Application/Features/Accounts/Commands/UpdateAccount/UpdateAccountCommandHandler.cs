using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Accounts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, AccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Account not found.");

        if (account.UserId != userId)
            throw new UnauthorizedException("Account does not belong to current user.");

        account.Name = request.Name.Trim();
        account.Type = request.Type;
        account.Balance = request.Balance;
        account.CreditLimit = request.CreditLimit;
        account.CutoffDay = request.CutoffDay;
        account.PaymentDay = request.PaymentDay;

        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto(account.Id, account.Name, account.Type, account.Balance, account.CreditLimit, account.CutoffDay, account.PaymentDay);
    }
}
