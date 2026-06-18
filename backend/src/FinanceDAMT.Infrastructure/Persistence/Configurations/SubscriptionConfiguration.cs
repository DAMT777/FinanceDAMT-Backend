using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Amount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.BillingCycle).HasConversion<int>();
        builder.Property(s => s.Icon).HasMaxLength(50);
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
