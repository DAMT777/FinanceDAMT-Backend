using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Survey> Surveys { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Category> Categories { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<SavingGoal> SavingGoals { get; }
    DbSet<SavingContribution> SavingContributions { get; }
    DbSet<AIRecommendation> AIRecommendations { get; }
    DbSet<FinancialScore> FinancialScores { get; }
    DbSet<Debt> Debts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
