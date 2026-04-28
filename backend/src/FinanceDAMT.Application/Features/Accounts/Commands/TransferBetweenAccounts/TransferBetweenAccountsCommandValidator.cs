using FluentValidation;

namespace FinanceDAMT.Application.Features.Accounts.Commands.TransferBetweenAccounts;

public sealed class TransferBetweenAccountsCommandValidator : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsCommandValidator()
    {
        RuleFor(x => x.FromAccountId).NotEmpty();
        RuleFor(x => x.ToAccountId).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.FromAccountId != x.ToAccountId)
            .WithMessage("Source and target accounts must be different.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero.");
    }
}
