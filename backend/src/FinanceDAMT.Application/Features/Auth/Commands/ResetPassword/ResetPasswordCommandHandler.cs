using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;

    public ResetPasswordCommandHandler(
        UserManager<User> userManager,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedException("Invalid password reset request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure("Token", e.Description));
            throw new Common.Exceptions.ValidationException(errors);
        }

        // Revoke all active refresh tokens for this user after password reset
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoked = DateTime.UtcNow;
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
