using FinanceDAMT.Application.Features.Accounts.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Accounts.Queries.GetNetWorth;

public sealed record GetNetWorthQuery : IRequest<NetWorthDto>;
