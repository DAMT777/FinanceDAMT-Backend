using FinanceDAMT.Application.Features.Accounts.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Accounts.Commands.UpdateAccount;

public sealed record UpdateAccountCommand(
    Guid Id,
    string Name,
    AccountType Type,
    decimal Balance,
    decimal? CreditLimit,
    int? CutoffDay,
    int? PaymentDay
) : IRequest<AccountDto>;
