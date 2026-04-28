using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RevokeTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            ?? throw new NotFoundException("Refresh token not found.");

        if (!token.IsActive)
            throw new UnauthorizedException("Token is already expired or revoked.");

        token.Revoked = DateTime.UtcNow;
        token.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
