using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Accounts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Accounts.Queries.GetAccountById;

public sealed class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAccountByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var account = await _context.Accounts
            .Where(a => a.Id == request.Id && a.UserId == userId)
            .Select(a => new AccountDto(a.Id, a.Name, a.Type, a.Balance, a.CreditLimit, a.CutoffDay, a.PaymentDay))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Account not found.");

        return account;
    }
}
