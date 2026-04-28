using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Accounts.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Accounts.Queries.GetNetWorth;

public sealed class GetNetWorthQueryHandler : IRequestHandler<GetNetWorthQuery, NetWorthDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNetWorthQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<NetWorthDto> Handle(GetNetWorthQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var accountAssets = await _context.Accounts
            .Where(a => a.UserId == userId)
            .SumAsync(a => (decimal?)a.Balance, cancellationToken) ?? 0m;

        var receivables = await _context.Debts
            .Where(d => d.UserId == userId && !d.IsPaid && d.Type == DebtType.Given)
            .SumAsync(d => (decimal?)d.Amount, cancellationToken) ?? 0m;

        var liabilities = await _context.Debts
            .Where(d => d.UserId == userId && !d.IsPaid && d.Type == DebtType.Received)
            .SumAsync(d => (decimal?)d.Amount, cancellationToken) ?? 0m;

        var assets = accountAssets + receivables;
        var netWorth = assets - liabilities;

        return new NetWorthDto(assets, liabilities, netWorth);
    }
}
