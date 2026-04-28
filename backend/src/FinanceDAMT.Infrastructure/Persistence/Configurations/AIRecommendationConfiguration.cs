using FinanceDAMT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceDAMT.Infrastructure.Persistence.Configurations;

public class AIRecommendationConfiguration : IEntityTypeConfiguration<AIRecommendation>
{
    public void Configure(EntityTypeBuilder<AIRecommendation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Content).IsRequired();
        builder.HasIndex(r => new { r.UserId, r.GeneratedAt });
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasOne(r => r.User)
            .WithMany(u => u.AIRecommendations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
