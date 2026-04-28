using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Queries.GetSavingGoals;

public sealed record GetSavingGoalsQuery : IRequest<IReadOnlyList<SavingGoalDto>>;
