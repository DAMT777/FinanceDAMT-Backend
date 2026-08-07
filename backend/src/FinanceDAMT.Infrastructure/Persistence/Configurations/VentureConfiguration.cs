using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class VentureConfiguration : IEntityTypeConfiguration<Venture>
{
    public void Configure(EntityTypeBuilder<Venture> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Icon).HasMaxLength(50);
        builder.Property(v => v.Description).HasMaxLength(500);
        builder.HasQueryFilter(v => !v.IsDeleted);

        builder.HasOne(v => v.User)
            .WithMany(u => u.Ventures)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Batches)
            .WithOne(b => b.Venture)
            .HasForeignKey(b => b.VentureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
