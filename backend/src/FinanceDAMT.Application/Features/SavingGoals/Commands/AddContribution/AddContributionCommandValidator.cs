using FluentValidation;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.AddContribution;

public sealed class AddContributionCommandValidator : AbstractValidator<AddContributionCommand>
{
    public AddContributionCommandValidator()
    {
        RuleFor(x => x.GoalId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Date).NotEqual(default(DateTime));
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
