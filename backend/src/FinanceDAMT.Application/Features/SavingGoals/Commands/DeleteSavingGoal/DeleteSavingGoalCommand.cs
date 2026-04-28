using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.DeleteSavingGoal;

public sealed record DeleteSavingGoalCommand(Guid Id) : IRequest<Unit>;
