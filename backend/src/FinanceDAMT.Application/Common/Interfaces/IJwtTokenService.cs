using System.Security.Claims;
using FinanceDAMT.Domain.Entities;

namespace FinanceDAMT.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
