using FinanceDAMT.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FinanceDAMT.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "COP";
    public FinancialProfile? FinancialProfile { get; set; }
    public bool BiometricEnabled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public Survey? Survey { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<SavingGoal> SavingGoals { get; set; } = new List<SavingGoal>();
    public ICollection<AIRecommendation> AIRecommendations { get; set; } = new List<AIRecommendation>();
    public ICollection<FinancialScore> FinancialScores { get; set; } = new List<FinancialScore>();
    public ICollection<Debt> Debts { get; set; } = new List<Debt>();
}
