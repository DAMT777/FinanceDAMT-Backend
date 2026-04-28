using FinanceDAMT.Application.Features.Accounts.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Accounts.Queries.GetAccountById;

public sealed record GetAccountByIdQuery(Guid Id) : IRequest<AccountDto>;
