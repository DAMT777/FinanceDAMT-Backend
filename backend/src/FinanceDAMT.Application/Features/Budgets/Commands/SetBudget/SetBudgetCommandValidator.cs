using FluentValidation;

namespace FinanceDAMT.Application.Features.Budgets.Commands.SetBudget;

public sealed class SetBudgetCommandValidator : AbstractValidator<SetBudgetCommand>
{
    public SetBudgetCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 3000);
    }
}
