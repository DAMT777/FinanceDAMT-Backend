using FluentValidation;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.BillingCycle).IsInEnum();
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
