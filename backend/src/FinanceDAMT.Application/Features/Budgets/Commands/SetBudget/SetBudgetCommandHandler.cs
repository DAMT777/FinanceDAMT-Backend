using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Budgets.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Budgets.Commands.SetBudget;

public sealed class SetBudgetCommandHandler : IRequestHandler<SetBudgetCommand, BudgetDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetBudgetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BudgetDto> Handle(SetBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId), cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(
                b => b.UserId == userId && b.CategoryId == request.CategoryId && b.Month == request.Month && b.Year == request.Year,
                cancellationToken);

        if (budget is null)
        {
            budget = new Budget
            {
                UserId = userId,
                CategoryId = request.CategoryId,
                Month = request.Month,
                Year = request.Year,
                MonthlyLimit = request.MonthlyLimit,
                AlertSent80 = false,
                AlertSent100 = false
            };

            _context.Budgets.Add(budget);
        }
        else
        {
            budget.MonthlyLimit = request.MonthlyLimit;
            budget.AlertSent80 = false;
            budget.AlertSent100 = false;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BudgetDto(
            budget.Id,
            budget.CategoryId,
            category.Name,
            budget.MonthlyLimit,
            budget.Month,
            budget.Year,
            budget.AlertSent80,
            budget.AlertSent100);
    }
}
