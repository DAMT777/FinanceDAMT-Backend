using FluentValidation;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.DeleteSavingGoal;

public sealed class DeleteSavingGoalCommandValidator : AbstractValidator<DeleteSavingGoalCommand>
{
    public DeleteSavingGoalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
