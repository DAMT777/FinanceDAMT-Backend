using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Queries.GetSavingGoalById;

public sealed record GetSavingGoalByIdQuery(Guid Id) : IRequest<SavingGoalDto>;
