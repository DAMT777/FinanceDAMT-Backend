using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class FinancialScoreConfiguration : IEntityTypeConfiguration<FinancialScore>
{
    public void Configure(EntityTypeBuilder<FinancialScore> builder)
    {
        builder.HasKey(fs => fs.Id);
        builder.Property(fs => fs.Breakdown).IsRequired().HasColumnType("text");
        builder.HasIndex(fs => new { fs.UserId, fs.Year, fs.Month }).IsUnique();
        builder.HasQueryFilter(fs => !fs.IsDeleted);

        builder.HasOne(fs => fs.User)
            .WithMany(u => u.FinancialScores)
            .HasForeignKey(fs => fs.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
