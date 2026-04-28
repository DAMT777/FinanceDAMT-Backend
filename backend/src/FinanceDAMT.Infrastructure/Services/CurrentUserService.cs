using System.Security.Claims;
using FinanceDAMT.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FinanceDAMT.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtClaimTypes.Sub);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
           ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtClaimTypes.Email);

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    private static class JwtClaimTypes
    {
        public const string Sub = "sub";
        public const string Email = "email";
    }
}
