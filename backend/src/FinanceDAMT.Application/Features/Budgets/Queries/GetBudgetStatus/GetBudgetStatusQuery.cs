using FinanceDAMT.Application.Features.Budgets.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Budgets.Queries.GetBudgetStatus;

public sealed record GetBudgetStatusQuery(int Month, int Year) : IRequest<IReadOnlyList<BudgetStatusDto>>;
