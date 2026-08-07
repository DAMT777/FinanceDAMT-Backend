using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Auth;
using FinanceDAMT.Application.Features.Auth.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DomainRefreshToken = FinanceDAMT.Domain.Entities.RefreshToken;

namespace FinanceDAMT.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
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

        if (!user.EmailConfirmed)
        {
            // Development-only: auto-confirm instead of gating on the email code,
            // so existing unverified accounts can sign in during local testing.
            var autoConfirm = _configuration.GetValue<bool>("AuthSettings:AutoConfirmEmail");
            if (autoConfirm)
            {
                user.EmailConfirmed = true;
                user.EmailVerificationCode = null;
                user.EmailVerificationCodeExpiresAt = null;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                var verificationCode = EmailVerification.GenerateCode();
                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationCodeExpiresAt = EmailVerification.ExpiryFromNow();
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Email verification code for {Email}: {Code}", user.Email, verificationCode);
                await _emailService.SendEmailVerificationAsync(user.Email!, user.Name, verificationCode, cancellationToken);
                throw new UnauthorizedException("EMAIL_NOT_VERIFIED");
            }
        }

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
