using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Auth.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using DomainRefreshToken = FinanceDAMT.Domain.Entities.RefreshToken;

namespace FinanceDAMT.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
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

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (user.IsDeleted)
            throw new UnauthorizedException("Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            throw new UnauthorizedException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        var expirationDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7);

        var refreshToken = new DomainRefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            Expires = DateTime.UtcNow.AddDays(expirationDays)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var expirationMinutes = _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes", 60);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresIn: expirationMinutes * 60,
            UserId: user.Id,
            Email: user.Email!,
            Name: user.Name
        );
    }
}
