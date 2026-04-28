using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class SavingContributionConfiguration : IEntityTypeConfiguration<SavingContribution>
{
    public void Configure(EntityTypeBuilder<SavingContribution> builder)
    {
        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Amount).HasColumnType("decimal(18,2)");
        builder.Property(sc => sc.Note).HasMaxLength(500);
        builder.HasIndex(sc => new { sc.GoalId, sc.Date });
        builder.HasQueryFilter(sc => !sc.IsDeleted);

        builder.HasOne(sc => sc.Goal)
            .WithMany(sg => sg.Contributions)
            .HasForeignKey(sc => sc.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
