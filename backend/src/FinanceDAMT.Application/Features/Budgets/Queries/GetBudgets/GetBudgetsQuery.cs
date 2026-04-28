using FinanceDAMT.Application.Features.Budgets.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Budgets.Queries.GetBudgets;

public sealed record GetBudgetsQuery(int Month, int Year) : IRequest<IReadOnlyList<BudgetDto>>;
