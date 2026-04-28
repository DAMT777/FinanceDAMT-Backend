using FinanceDAMT.Domain.Enums;
using FluentValidation;

namespace FinanceDAMT.Application.Features.Transactions.Commands.CreateRecurringTransaction;

public sealed class CreateRecurringTransactionCommandValidator : AbstractValidator<CreateRecurringTransactionCommand>
{
    public CreateRecurringTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.Type)
            .Must(type => type is TransactionType.Income or TransactionType.Expense)
            .WithMessage("Type must be Income or Expense for this endpoint.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Date)
            .NotEqual(default(DateTime)).WithMessage("Date is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
