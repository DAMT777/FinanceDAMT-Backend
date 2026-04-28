using FluentValidation;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.UpdateSavingGoal;

public sealed class UpdateSavingGoalCommandValidator : AbstractValidator<UpdateSavingGoalCommand>
{
    public UpdateSavingGoalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(50);
    }
}
