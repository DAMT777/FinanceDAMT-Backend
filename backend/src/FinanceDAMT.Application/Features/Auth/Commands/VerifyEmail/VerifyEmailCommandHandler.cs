using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Auth.DTOs;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DomainRefreshToken = FinanceDAMT.Domain.Entities.RefreshToken;

namespace FinanceDAMT.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public VerifyEmailCommandHandler(
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

    public async Task<AuthResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted)
            throw new UnauthorizedException("Código inválido o expirado.");

        if (!user.EmailConfirmed)
        {
            if (string.IsNullOrEmpty(user.EmailVerificationCode) || user.EmailVerificationCode != request.Code)
                throw new UnauthorizedException("Código inválido o expirado.");

            if (user.EmailVerificationCodeExpiresAt is null || user.EmailVerificationCodeExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("El código expiró. Solicita uno nuevo.");

            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationCodeExpiresAt = null;
            await _userManager.UpdateAsync(user);
        }

        var hasAccount = await _context.Accounts.AnyAsync(a => a.UserId == user.Id, cancellationToken);
        if (!hasAccount)
        {
            _context.Accounts.Add(new Account
            {
                UserId = user.Id,
                Name = "Efectivo",
                Type = AccountType.Cash,
                Balance = 0m
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        var expirationDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7);

        _context.RefreshTokens.Add(new DomainRefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            Expires = DateTime.UtcNow.AddDays(expirationDays)
        });

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
