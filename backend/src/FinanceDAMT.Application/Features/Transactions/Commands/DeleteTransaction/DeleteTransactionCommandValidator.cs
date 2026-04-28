using FluentValidation;

namespace FinanceDAMT.Application.Features.Transactions.Commands.DeleteTransaction;

public sealed class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
