using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class SavingGoalConfiguration : IEntityTypeConfiguration<SavingGoal>
{
    public void Configure(EntityTypeBuilder<SavingGoal> builder)
    {
        builder.HasKey(sg => sg.Id);
        builder.Property(sg => sg.Name).IsRequired().HasMaxLength(200);
        builder.Property(sg => sg.TargetAmount).HasColumnType("decimal(18,2)");
        builder.Property(sg => sg.CurrentAmount).HasColumnType("decimal(18,2)");
        builder.Property(sg => sg.Icon).HasMaxLength(50);
        builder.Property(sg => sg.MilestonesReached).HasDefaultValue(0);
        builder.HasQueryFilter(sg => !sg.IsDeleted);

        builder.HasOne(sg => sg.User)
            .WithMany(u => u.SavingGoals)
            .HasForeignKey(sg => sg.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sg => sg.Contributions)
            .WithOne(sc => sc.Goal)
            .HasForeignKey(sc => sc.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
