using FluentValidation;

namespace FinanceDAMT.Application.Features.Budgets.Commands.DeleteBudget;

public sealed class DeleteBudgetCommandValidator : AbstractValidator<DeleteBudgetCommand>
{
    public DeleteBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
