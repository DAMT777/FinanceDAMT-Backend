using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class VentureBatchConfiguration : IEntityTypeConfiguration<VentureBatch>
{
    public void Configure(EntityTypeBuilder<VentureBatch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Label).HasMaxLength(120);
        builder.Property(b => b.Investment).HasColumnType("decimal(18,2)");
        builder.Property(b => b.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Notes).HasMaxLength(500);
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
