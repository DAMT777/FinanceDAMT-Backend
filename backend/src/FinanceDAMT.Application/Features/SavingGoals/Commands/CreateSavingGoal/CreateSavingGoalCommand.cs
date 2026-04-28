using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.CreateSavingGoal;

public sealed record CreateSavingGoalCommand(
    string Name,
    decimal TargetAmount,
    DateTime Deadline,
    string Icon
) : IRequest<SavingGoalDto>;
