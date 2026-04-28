using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.EmploymentType).IsRequired().HasMaxLength(100);
        builder.Property(s => s.MonthlyIncome).HasColumnType("decimal(18,2)");
        builder.Property(s => s.FixedExpenses).HasColumnType("decimal(18,2)");
        builder.Property(s => s.FinancialGoals).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.SavingsLevel).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.UserId).IsUnique();
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasOne(s => s.User)
            .WithOne(u => u.Survey)
            .HasForeignKey<Survey>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
