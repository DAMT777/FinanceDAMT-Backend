using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Auth.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DomainRefreshToken = FinanceDAMT.Domain.Entities.RefreshToken;

namespace FinanceDAMT.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new UnauthorizedException("Refresh token is expired or revoked.");

        var user = existing.User;
        if (user.IsDeleted)
            throw new UnauthorizedException("Invalid refresh token.");

        // Rotate: revoke old, issue new
        existing.Revoked = DateTime.UtcNow;
        existing.IsRevoked = true;

        var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var expirationDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7);

        var newRefreshToken = new DomainRefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            Expires = DateTime.UtcNow.AddDays(expirationDays)
        };

        existing.ReplacedByToken = newRefreshTokenString;
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var expirationMinutes = _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes", 60);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshTokenString,
            ExpiresIn: expirationMinutes * 60,
            UserId: user.Id,
            Email: user.Email!,
            Name: user.Name
        );
    }
}
