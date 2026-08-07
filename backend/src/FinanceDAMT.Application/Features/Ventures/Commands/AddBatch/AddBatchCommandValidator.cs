using FluentValidation;

namespace FinanceDAMT.Application.Features.Ventures.Commands.AddBatch;

public sealed class AddBatchCommandValidator : AbstractValidator<AddBatchCommand>
{
    public AddBatchCommandValidator()
    {
        RuleFor(x => x.VentureId).NotEmpty();
        RuleFor(x => x.Label).MaximumLength(120);
        RuleFor(x => x.Investment).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitsProduced).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Income).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
