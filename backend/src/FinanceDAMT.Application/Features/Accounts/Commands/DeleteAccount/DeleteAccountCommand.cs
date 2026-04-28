using MediatR;

namespace FinanceDAMT.Application.Features.Accounts.Commands.DeleteAccount;

public sealed record DeleteAccountCommand(Guid Id) : IRequest<Unit>;
