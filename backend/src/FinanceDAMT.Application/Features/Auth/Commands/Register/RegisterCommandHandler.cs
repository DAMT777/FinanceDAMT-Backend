using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Auth.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinanceDAMT.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResultDto>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RegisterResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        // Development-only shortcut: skip email verification so the account is
        // usable immediately. Production leaves this false and the real code +
        // email flow runs. Toggled via AuthSettings:AutoConfirmEmail.
        var autoConfirm = _configuration.GetValue<bool>("AuthSettings:AutoConfirmEmail");
        var code = EmailVerification.GenerateCode();

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            UserName = request.Email,
            EmailConfirmed = autoConfirm,
            EmailVerificationCode = autoConfirm ? null : code,
            EmailVerificationCodeExpiresAt = autoConfirm ? null : EmailVerification.ExpiryFromNow()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure("Password", e.Description));
            throw new Common.Exceptions.ValidationException(errors);
        }

        if (!autoConfirm)
        {
            _logger.LogInformation("Email verification code for {Email}: {Code}", request.Email, code);
            await _emailService.SendEmailVerificationAsync(user.Email!, user.Name, code, cancellationToken);
        }

        return new RegisterResultDto(user.Email!, !autoConfirm);
    }
}
