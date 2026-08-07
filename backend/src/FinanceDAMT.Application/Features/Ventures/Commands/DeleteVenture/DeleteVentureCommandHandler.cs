using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Ventures.Commands.DeleteVenture;

public sealed class DeleteVentureCommandHandler : IRequestHandler<DeleteVentureCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteVentureCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteVentureCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = await _context.Ventures
            .Include(v => v.Batches)
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Venture not found.");

        venture.IsDeleted = true;
        venture.DeletedAt = DateTime.UtcNow;
        foreach (var batch in venture.Batches)
        {
            batch.IsDeleted = true;
            batch.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
