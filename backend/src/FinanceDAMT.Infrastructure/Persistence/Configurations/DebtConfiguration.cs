using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(500);
        builder.Property(d => d.Amount).HasColumnType("decimal(18,2)");
        builder.Property(d => d.CreditorOrDebtor).IsRequired().HasMaxLength(200);
        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Debts)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
