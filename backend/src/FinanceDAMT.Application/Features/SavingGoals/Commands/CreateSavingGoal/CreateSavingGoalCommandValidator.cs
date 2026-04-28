using FluentValidation;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.CreateSavingGoal;

public sealed class CreateSavingGoalCommandValidator : AbstractValidator<CreateSavingGoalCommand>
{
    public CreateSavingGoalCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.Deadline).GreaterThan(DateTime.UtcNow.Date.AddDays(-1));
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(50);
    }
}
