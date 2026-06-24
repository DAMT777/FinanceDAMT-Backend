namespace FinanceDAMT.Application.Features.Auth;

public static class EmailVerification
{
    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);

    public static string GenerateCode() => Random.Shared.Next(0, 1_000_000).ToString("D6");

    public static DateTime ExpiryFromNow() => DateTime.UtcNow.Add(CodeLifetime);
}
