using FluentValidation;

namespace FinanceDAMT.Application.Features.Ventures.Commands.RegisterSale;

public sealed class RegisterSaleCommandValidator : AbstractValidator<RegisterSaleCommand>
{
    public RegisterSaleCommandValidator()
    {
        RuleFor(x => x.VentureId).NotEmpty();
        RuleFor(x => x.BatchId).NotEmpty();
        RuleFor(x => x.Units).GreaterThan(0);
    }
}
