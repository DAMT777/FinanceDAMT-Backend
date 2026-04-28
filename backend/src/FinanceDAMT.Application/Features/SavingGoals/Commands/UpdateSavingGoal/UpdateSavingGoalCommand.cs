using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.UpdateSavingGoal;

public sealed record UpdateSavingGoalCommand(
    Guid Id,
    string Name,
    decimal TargetAmount,
    DateTime Deadline,
    string Icon
) : IRequest<SavingGoalDto>;
