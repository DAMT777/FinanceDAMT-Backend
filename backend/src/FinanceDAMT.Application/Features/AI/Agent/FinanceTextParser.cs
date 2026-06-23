using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.AI.Agent;

public sealed record ParsedExpense(TransactionType Type, decimal Amount, string CategoryName, string Description);

public sealed record ParsedGoalContribution(decimal? Amount, decimal? PercentOfBalance);

public static class FinanceTextParser
{
    private static readonly string[] ReportTriggers =
    {
        "reporte", "report", "resumen", "informe", "como voy", "como van", "como estoy",
        "estado de cuenta", "balance de", "cuanto gaste", "cuanto gane", "cuanto ingrese", "mis finanzas"
    };

    private static readonly string[] ExpenseVerbs =
    {
        "compre", "gaste", "pague", "gasto de", "me gaste", "compra de", "pagar ", "gastar "
    };

    private static readonly string[] IncomeVerbs =
    {
        "recibi", "me pagaron", "gane", "ingreso de", "cobre", "me ingresaron", "deposit", "me deposit"
    };

    private static readonly (string Keyword, string Category)[] ExpenseMap =
    {
        ("gaseosa", "Food"), ("bebida", "Food"), ("comida", "Food"), ("almuerzo", "Food"), ("cena", "Food"),
        ("desayuno", "Food"), ("restaurante", "Food"), ("mercado", "Food"), ("supermercado", "Food"), ("cafe", "Food"),
        ("pizza", "Food"), ("hamburguesa", "Food"), ("snack", "Food"), ("lunch", "Food"), ("dinner", "Food"),
        ("groceries", "Food"), ("food", "Food"),
        ("gasolina", "Transport"), ("uber", "Transport"), ("taxi", "Transport"), ("transporte", "Transport"),
        ("pasaje", "Transport"), ("metro", "Transport"), ("fuel", "Transport"), ("peaje", "Transport"),
        ("arriendo", "Housing"), ("alquiler", "Housing"), ("renta", "Housing"), ("hogar", "Housing"), ("rent", "Housing"),
        ("servicios", "Housing"),
        ("salud", "Health"), ("medicina", "Health"), ("medico", "Health"), ("farmacia", "Health"), ("doctor", "Health"),
        ("health", "Health"), ("pharmacy", "Health"),
        ("cine", "Entertainment"), ("ocio", "Entertainment"), ("juego", "Entertainment"), ("entretenimiento", "Entertainment"),
        ("movie", "Entertainment"), ("game", "Entertainment"), ("bar", "Entertainment"),
        ("educacion", "Education"), ("curso", "Education"), ("libro", "Education"), ("universidad", "Education"),
        ("colegio", "Education"), ("education", "Education"), ("course", "Education"), ("book", "Education"),
        ("ropa", "Clothing"), ("camisa", "Clothing"), ("zapatos", "Clothing"), ("clothing", "Clothing"),
        ("shoes", "Clothing"), ("pantalon", "Clothing"),
        ("suscripcion", "Subscriptions"), ("subscription", "Subscriptions"), ("netflix", "Subscriptions"),
        ("spotify", "Subscriptions"), ("plan", "Subscriptions")
    };

    private static readonly string[] ContributionTriggers =
    {
        "asigna", "asignar", "asigne", "aporta", "aportar", "aporte", "abona", "abonar",
        "destina", "destinar", "guarda para", "ahorra para", "anade a", "suma a"
    };

    private static readonly string[] GoalWords = { "meta", "objetivo", "goal", "proposito" };

    private static readonly (string Keyword, string Category)[] IncomeMap =
    {
        ("salario", "Salary"), ("sueldo", "Salary"), ("nomina", "Salary"), ("salary", "Salary"), ("paga", "Salary"),
        ("freelance", "Freelance"), ("proyecto", "Freelance"),
        ("inversion", "Investment"), ("dividendo", "Investment"), ("investment", "Investment"), ("interes", "Investment")
    };

        public static string Normalize(string text)
    {
        var lower = (text ?? string.Empty).ToLowerInvariant();
        var decomposed = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

        public static ReportPeriod? TryParseReportPeriod(string message)
    {
        var n = Normalize(message);
        if (!ReportTriggers.Any(trigger => n.Contains(trigger)))
            return null;

        if (n.Contains("semana pasada") || n.Contains("semana anterior") || n.Contains("last week"))
            return ReportPeriod.LastWeek;
        if (n.Contains("esta semana") || n.Contains("de la semana") || n.Contains("this week") || n.Contains("semanal"))
            return ReportPeriod.ThisWeek;
        if (n.Contains("mes pasado") || n.Contains("mes anterior") || n.Contains("last month"))
            return ReportPeriod.LastMonth;
        if (n.Contains("este mes") || n.Contains("del mes") || n.Contains("this month") || n.Contains("mensual"))
            return ReportPeriod.ThisMonth;
        if (n.Contains("ayer") || n.Contains("yesterday"))
            return ReportPeriod.Yesterday;
        if (n.Contains("hoy") || n.Contains("today") || n.Contains("del dia") || n.Contains("de hoy"))
            return ReportPeriod.Today;
        if (n.Contains("ultimos 7") || n.Contains("7 dias") || n.Contains("last 7"))
            return ReportPeriod.Last7Days;
        if (n.Contains("ultimos 30") || n.Contains("30 dias") || n.Contains("last 30"))
            return ReportPeriod.Last30Days;

        return ReportPeriod.ThisMonth;
    }

        public static ParsedExpense? TryParseExpense(string message)
    {
        var n = Normalize(message);
        var amount = ParseAmount(n);
        if (amount <= 0)
            return null;

        TransactionType? type = null;
        if (IncomeVerbs.Any(verb => n.Contains(verb)))
            type = TransactionType.Income;
        else if (ExpenseVerbs.Any(verb => n.Contains(verb)))
            type = TransactionType.Expense;

        if (type is null)
            return null;

        var category = ResolveCategoryName(n, type.Value);
        var description = BuildDescription(message);
        return new ParsedExpense(type.Value, amount, category, description);
    }

        public static ParsedGoalContribution? TryParseGoalContribution(string message)
    {
        var n = Normalize(message);
        if (!ContributionTriggers.Any(trigger => n.Contains(trigger)))
            return null;
        if (!GoalWords.Any(word => n.Contains(word)))
            return null;

        var pct = Regex.Match(n, @"(\d+(?:[.,]\d+)?)\s*(?:%|por\s*ciento|porciento)");
        if (pct.Success && TryBaseNumber(pct.Groups[1].Value, out var percent) && percent > 0)
            return new ParsedGoalContribution(null, percent);

        var amount = ParseAmount(n);
        if (amount > 0)
            return new ParsedGoalContribution(amount, null);

        return null;
    }

    public static string ResolveCategoryName(string normalizedText, TransactionType type)
    {
        var map = type == TransactionType.Income ? IncomeMap : ExpenseMap;
        foreach (var (keyword, category) in map)
        {
            if (normalizedText.Contains(keyword))
                return category;
        }
        return type == TransactionType.Income ? "Other income" : "Other expense";
    }

        public static decimal ParseAmount(string normalizedText)
    {
        var million = Regex.Match(normalizedText, @"(\d+(?:[.,]\d+)*)\s*(?:millones|millon)");
        if (million.Success && TryBaseNumber(million.Groups[1].Value, out var millionBase))
            return Math.Round(millionBase * 1_000_000m);

        var thousand = Regex.Match(normalizedText, @"(\d+(?:[.,]\d+)*)\s*(?:mil|k)\b");
        if (thousand.Success && TryBaseNumber(thousand.Groups[1].Value, out var thousandBase))
            return Math.Round(thousandBase * 1_000m);

        var plain = Regex.Match(normalizedText, @"\d{1,3}(?:\.\d{3})+|\d+");
        if (plain.Success &&
            decimal.TryParse(plain.Value.Replace(".", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out var plainValue))
            return plainValue;

        return 0m;
    }

        private static bool TryBaseNumber(string raw, out decimal value)
    {
        var normalized = raw.Replace(",", ".");
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static string BuildDescription(string original)
    {
        var trimmed = (original ?? string.Empty).Trim();
        return trimmed.Length > 120 ? trimmed[..120] : trimmed;
    }
}
