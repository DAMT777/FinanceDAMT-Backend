using FluentValidation;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.RemoveContribution;

public sealed class RemoveContributionCommandValidator : AbstractValidator<RemoveContributionCommand>
{
    public RemoveContributionCommandValidator()
    {
        RuleFor(x => x.GoalId).NotEmpty();
        RuleFor(x => x.ContributionId).NotEmpty();
    }
}
