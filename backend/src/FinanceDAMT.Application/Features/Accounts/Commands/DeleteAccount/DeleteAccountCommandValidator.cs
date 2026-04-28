using FluentValidation;

namespace FinanceDAMT.Application.Features.Accounts.Commands.DeleteAccount;

public sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
