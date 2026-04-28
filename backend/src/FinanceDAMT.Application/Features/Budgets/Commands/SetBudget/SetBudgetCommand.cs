using FinanceDAMT.Application.Features.Budgets.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Budgets.Commands.SetBudget;

public sealed record SetBudgetCommand(
    Guid CategoryId,
    decimal MonthlyLimit,
    int Month,
    int Year
) : IRequest<BudgetDto>;
