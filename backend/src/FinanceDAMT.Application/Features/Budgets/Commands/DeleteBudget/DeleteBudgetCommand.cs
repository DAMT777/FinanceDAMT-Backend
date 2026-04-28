using MediatR;

namespace FinanceDAMT.Application.Features.Budgets.Commands.DeleteBudget;

public sealed record DeleteBudgetCommand(Guid Id) : IRequest<Unit>;
