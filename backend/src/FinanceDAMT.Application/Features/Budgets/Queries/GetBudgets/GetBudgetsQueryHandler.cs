using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Budgets.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Budgets.Queries.GetBudgets;

public sealed class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, IReadOnlyList<BudgetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBudgetsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BudgetDto>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        return await _context.Budgets
            .Where(b => b.UserId == userId && b.Month == request.Month && b.Year == request.Year)
            .OrderBy(b => b.Category.Name)
            .Select(b => new BudgetDto(
                b.Id,
                b.CategoryId,
                b.Category.Name,
                b.MonthlyLimit,
                b.Month,
                b.Year,
                b.AlertSent80,
                b.AlertSent100))
            .ToListAsync(cancellationToken);
    }
}
