using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinanceDAMT.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Always return success — never reveal whether email exists
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted)
            return Unit.Value;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var baseUrl = _configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:19006";
        var resetLink = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

        _logger.LogInformation("Password reset requested for {Email}", request.Email);

        await _emailService.SendPasswordResetEmailAsync(
            user.Email!,
            user.Name,
            resetLink,
            cancellationToken);

        return Unit.Value;
    }
}
