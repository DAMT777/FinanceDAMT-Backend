using FluentValidation;

namespace FinanceDAMT.Application.Features.Ventures.Commands.CreateVenture;

public sealed class CreateVentureCommandValidator : AbstractValidator<CreateVentureCommand>
{
    public CreateVentureCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Icon).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
