using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Budgets.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Budgets.Queries.GetBudgetStatus;

public sealed class GetBudgetStatusQueryHandler : IRequestHandler<GetBudgetStatusQuery, IReadOnlyList<BudgetStatusDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBudgetStatusQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BudgetStatusDto>> Handle(GetBudgetStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        var monthStart = new DateTime(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId && b.Month == request.Month && b.Year == request.Year)
            .Select(b => new
            {
                b.Id,
                b.CategoryId,
                CategoryName = b.Category.Name,
                b.MonthlyLimit,
                b.AlertSent80,
                b.AlertSent100
            })
            .ToListAsync(cancellationToken);

        var expenses = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.Type == TransactionType.Expense &&
                t.Date >= monthStart &&
                t.Date < monthEnd)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Spent = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Spent, cancellationToken);

        return budgets
            .Select(b =>
            {
                var spent = expenses.TryGetValue(b.CategoryId, out var value) ? value : 0m;
                var percentage = b.MonthlyLimit <= 0 ? 0m : Math.Round((spent / b.MonthlyLimit) * 100m, 2);

                return new BudgetStatusDto(
                    b.Id,
                    b.CategoryId,
                    b.CategoryName,
                    b.MonthlyLimit,
                    spent,
                    percentage,
                    b.AlertSent80,
                    b.AlertSent100);
            })
            .OrderBy(x => x.CategoryName)
            .ToList();
    }
}
