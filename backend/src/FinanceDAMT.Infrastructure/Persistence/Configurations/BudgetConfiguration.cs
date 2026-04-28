using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.MonthlyLimit).HasColumnType("decimal(18,2)");
        builder.Property(b => b.AlertSent80).HasDefaultValue(false);
        builder.Property(b => b.AlertSent100).HasDefaultValue(false);
        builder.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year }).IsUnique();
        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Budgets)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Category)
            .WithMany(c => c.Budgets)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
