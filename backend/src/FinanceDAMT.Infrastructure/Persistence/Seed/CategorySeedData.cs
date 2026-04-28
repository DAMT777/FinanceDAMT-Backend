using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Infrastructure.Persistence.Seed;

public static class CategorySeedData
{
    private static readonly (string Name, string Icon, string Color, CategoryType Type)[] Defaults =
    [
        ("Salary", "wallet", "#16A34A", CategoryType.Income),
        ("Freelance", "briefcase", "#0284C7", CategoryType.Income),
        ("Investment", "trending-up", "#7C3AED", CategoryType.Income),
        ("Other income", "plus-circle", "#059669", CategoryType.Income),

        ("Food", "utensils", "#EF4444", CategoryType.Expense),
        ("Transport", "car", "#F97316", CategoryType.Expense),
        ("Housing", "home", "#6366F1", CategoryType.Expense),
        ("Health", "heart-pulse", "#DC2626", CategoryType.Expense),
        ("Entertainment", "film", "#8B5CF6", CategoryType.Expense),
        ("Education", "book-open", "#2563EB", CategoryType.Expense),
        ("Clothing", "shirt", "#EC4899", CategoryType.Expense),
        ("Subscriptions", "receipt", "#0EA5E9", CategoryType.Expense),
        ("Other expense", "circle", "#6B7280", CategoryType.Expense)
    ];

    public static async Task SeedGlobalDefaultsAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        foreach (var item in Defaults)
        {
            var exists = await context.Categories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.UserId == null && c.Name == item.Name && c.Type == item.Type, cancellationToken);

            if (exists)
                continue;

            context.Categories.Add(new Category
            {
                UserId = null,
                Name = item.Name,
                Icon = item.Icon,
                Color = item.Color,
                Type = item.Type
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
