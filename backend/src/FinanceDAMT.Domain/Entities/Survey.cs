using FinanceDAMT.Domain.Common;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Domain.Entities;

public class Survey : BaseEntity
{
    public Guid UserId { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public int Dependents { get; set; }
    public decimal FixedExpenses { get; set; }
    public string FinancialGoals { get; set; } = string.Empty;
    public string SavingsLevel { get; set; } = string.Empty;
    public FinancialProfile CalculatedProfile { get; set; }

    public User User { get; set; } = null!;
}
