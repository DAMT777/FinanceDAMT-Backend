using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FinanceDAMT.Application.Features.Auth.Commands.ResendVerification;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, Unit>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResendVerificationCommandHandler> _logger;

    public ResendVerificationCommandHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<ResendVerificationCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted || user.EmailConfirmed)
            return Unit.Value;

        var code = Auth.EmailVerification.GenerateCode();
        user.EmailVerificationCode = code;
        user.EmailVerificationCodeExpiresAt = Auth.EmailVerification.ExpiryFromNow();
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Email verification code (resent) for {Email}: {Code}", request.Email, code);

        await _emailService.SendEmailVerificationAsync(user.Email!, user.Name, code, cancellationToken);

        return Unit.Value;
    }
}
