using System.Globalization;
using System.Text;
using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.AI.Queries.GenerateReport;

public sealed class GenerateReportQueryHandler : IRequestHandler<GenerateReportQuery, FinancialReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    private static readonly NumberFormatInfo MoneyFormat = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalSeparator = ",",
        NumberDecimalDigits = 0
    };

    public GenerateReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FinancialReportDto> Handle(GenerateReportQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var (start, end, label) = ResolveRange(request.Period);

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Date >= start && t.Date < end)
            .Select(t => new { t.Type, t.Amount, CategoryName = t.Category.Name })
            .ToListAsync(cancellationToken);

        var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var net = income - expenses;

        var topCategories = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryName)
            .Select(g => new ReportCategoryDto(
                g.Key,
                g.Sum(x => x.Amount),
                expenses > 0 ? (double)Math.Round(g.Sum(x => x.Amount) / expenses * 100m, 1) : 0))
            .OrderByDescending(c => c.Amount)
            .Take(5)
            .ToList();

        var message = BuildMessage(label, income, expenses, net, transactions.Count, topCategories);

        return new FinancialReportDto(
            request.Period,
            label,
            start,
            end,
            income,
            expenses,
            net,
            transactions.Count,
            topCategories,
            message);
    }

    private static string BuildMessage(
        string label,
        decimal income,
        decimal expenses,
        decimal net,
        int count,
        IReadOnlyList<ReportCategoryDto> categories)
    {
        if (count == 0)
            return $"📊 Reporte de {label}\n\nNo registré movimientos en {label}. Cuéntame un gasto o ingreso para empezar a llevar el control.";

        var sb = new StringBuilder();
        sb.Append("📊 Reporte de ").Append(label).Append("\n\n");
        sb.Append("📈 Ingresos: ").Append(Money(income)).Append('\n');
        sb.Append("📉 Gastos: ").Append(Money(expenses)).Append('\n');
        sb.Append("💰 Balance: ").Append(net >= 0 ? "+" : "-").Append(Money(Math.Abs(net))).Append('\n');
        sb.Append("🧾 ").Append(count).Append(count == 1 ? " transacción" : " transacciones");

        if (categories.Count > 0)
        {
            sb.Append("\n\nPrincipales gastos:");
            var rank = 1;
            foreach (var category in categories)
            {
                sb.Append('\n').Append(rank++).Append(". ")
                  .Append(category.CategoryName).Append(" — ").Append(Money(category.Amount))
                  .Append(" (").Append(category.Percentage.ToString("0.#", CultureInfo.InvariantCulture)).Append("%)");
            }
        }

        return sb.ToString();
    }

    private static string Money(decimal value) => "$" + value.ToString("#,##0", MoneyFormat);

    private static (DateTime Start, DateTime End, string Label) ResolveRange(ReportPeriod period)
    {
        var today = DateTime.UtcNow.Date;

        switch (period)
        {
            case ReportPeriod.Today:
                return (today, today.AddDays(1), "hoy");
            case ReportPeriod.Yesterday:
                return (today.AddDays(-1), today, "ayer");
            case ReportPeriod.ThisWeek:
            {
                var start = StartOfWeek(today);
                return (start, start.AddDays(7), "esta semana");
            }
            case ReportPeriod.LastWeek:
            {
                var start = StartOfWeek(today).AddDays(-7);
                return (start, start.AddDays(7), "la semana pasada");
            }
            case ReportPeriod.LastMonth:
            {
                var firstThisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return (firstThisMonth.AddMonths(-1), firstThisMonth, "el mes pasado");
            }
            case ReportPeriod.Last7Days:
                return (today.AddDays(-6), today.AddDays(1), "los últimos 7 días");
            case ReportPeriod.Last30Days:
                return (today.AddDays(-29), today.AddDays(1), "los últimos 30 días");
            case ReportPeriod.ThisMonth:
            default:
            {
                var first = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return (first, first.AddMonths(1), "este mes");
            }
        }
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        // Monday as the first day of the week.
        var diff = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-diff);
    }
}
