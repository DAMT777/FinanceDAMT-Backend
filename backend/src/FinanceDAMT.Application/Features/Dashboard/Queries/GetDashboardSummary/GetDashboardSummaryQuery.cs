using FinanceDAMT.Application.Features.Dashboard.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(int Month, int Year) : IRequest<DashboardSummaryDto>;
