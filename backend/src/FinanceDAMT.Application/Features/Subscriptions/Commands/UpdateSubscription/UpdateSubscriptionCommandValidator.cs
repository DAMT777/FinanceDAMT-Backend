using FluentValidation;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.UpdateSubscription;

public sealed class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.BillingCycle).IsInEnum();
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
