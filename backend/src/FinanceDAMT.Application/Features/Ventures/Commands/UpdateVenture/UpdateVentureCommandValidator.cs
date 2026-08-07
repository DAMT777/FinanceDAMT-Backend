using FluentValidation;

namespace FinanceDAMT.Application.Features.Ventures.Commands.UpdateVenture;

public sealed class UpdateVentureCommandValidator : AbstractValidator<UpdateVentureCommand>
{
    public UpdateVentureCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Icon).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
