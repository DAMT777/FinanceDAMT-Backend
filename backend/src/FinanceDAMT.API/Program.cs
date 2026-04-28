using FinanceDAMT.Application;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Infrastructure;
using FinanceDAMT.API.Middleware;
using FinanceDAMT.API.Security;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

// ── Application + Infrastructure DI ──────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinanceDAMT API",
        Version = "v1",
        Description = "Personal Finance Management API"
    });

    // JWT bearer support in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without 'Bearer ' prefix)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinanceDAMT API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new List<IDashboardAuthorizationFilter>
    {
        new HangfireDashboardAuthorizationFilter(app.Environment)
    }
});

RecurringJob.AddOrUpdate<IFinanceRecurringJobs>(
    "monthly-summary-job",
    job => job.GenerateMonthlySummariesAsync(CancellationToken.None),
    builder.Configuration["BackgroundJobs:MonthlySummaryCron"] ?? "0 7 1 * *");

RecurringJob.AddOrUpdate<IFinanceRecurringJobs>(
    "daily-budget-check-job",
    job => job.RunDailyBudgetChecksAsync(CancellationToken.None),
    builder.Configuration["BackgroundJobs:DailyBudgetCheckCron"] ?? "0 8 * * *");

RecurringJob.AddOrUpdate<IFinanceRecurringJobs>(
    "inactive-user-reminder-job",
    job => job.SendInactiveUserRemindersAsync(CancellationToken.None),
    builder.Configuration["BackgroundJobs:InactiveReminderCron"] ?? "0 9 * * *");

RecurringJob.AddOrUpdate<IFinanceRecurringJobs>(
    "monthly-financial-score-job",
    job => job.CalculateMonthlyScoresAsync(CancellationToken.None),
    builder.Configuration["BackgroundJobs:MonthlyScoreCron"] ?? "0 6 1 * *");

app.MapControllers();

// ── Auto-migrate on startup (dev only) ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<FinanceDAMT.Infrastructure.Persistence.ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await FinanceDAMT.Infrastructure.Persistence.Seed.CategorySeedData.SeedGlobalDefaultsAsync(dbContext);
}

app.Run();
