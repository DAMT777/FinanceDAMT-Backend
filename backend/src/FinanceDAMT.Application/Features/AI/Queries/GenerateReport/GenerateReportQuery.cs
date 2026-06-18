using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Queries.GenerateReport;

public sealed record GenerateReportQuery(ReportPeriod Period) : IRequest<FinancialReportDto>;
