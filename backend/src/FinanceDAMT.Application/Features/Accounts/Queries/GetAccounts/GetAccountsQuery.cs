using FinanceDAMT.Application.Features.Accounts.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Accounts.Queries.GetAccounts;

public sealed record GetAccountsQuery : IRequest<IReadOnlyList<AccountDto>>;
