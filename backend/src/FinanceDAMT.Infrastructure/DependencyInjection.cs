using System.Text;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Infrastructure.Jobs;
using FinanceDAMT.Infrastructure.Persistence;
using FinanceDAMT.Infrastructure.Persistence.Repositories;
using FinanceDAMT.Infrastructure.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace FinanceDAMT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────────────────
        var dbProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";
        var defaultConnection = configuration["ConnectionStrings:DefaultConnection"];

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                var connString = defaultConnection ?? configuration["ConnectionStrings:SqlServer"]
                    ?? throw new InvalidOperationException("SqlServer connection string not configured.");
                options.UseSqlServer(connString);
            }
            else
            {
                var connString = defaultConnection ?? configuration["ConnectionStrings:PostgreSQL"]
                    ?? throw new InvalidOperationException("PostgreSQL connection string not configured.");
                options.UseNpgsql(connString);
            }
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // ── Identity ─────────────────────────────────────────────────────────
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ── JWT Authentication ────────────────────────────────────────────────
        var secret = configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // ── Services ─────────────────────────────────────────────────────────
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IBudgetAlertService, BudgetAlertService>();
        services.AddScoped<IFinanceRecurringJobs, FinanceRecurringJobs>();
        services.AddHttpClient<IAIService, GroqAIService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // ── Redis ─────────────────────────────────────────────────────────────
        var redisConnection = configuration["RedisSettings:Connection"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<ICacheService, CacheService>();

        // ── Hangfire ─────────────────────────────────────────────────────────
        services.AddHangfire(cfg => cfg.UseMemoryStorage());
        services.AddHangfireServer();

        return services;
    }
}
