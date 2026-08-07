using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Ventures.Commands.DeleteBatch;

public sealed class DeleteBatchCommandHandler : IRequestHandler<DeleteBatchCommand, VentureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteBatchCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VentureDto> Handle(DeleteBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = await _context.Ventures
            .Include(v => v.Batches)
            .FirstOrDefaultAsync(v => v.Id == request.VentureId && v.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Venture not found.");

        var batch = venture.Batches.FirstOrDefault(b => b.Id == request.BatchId)
            ?? throw new NotFoundException("Batch not found.");

        // Soft delete: mark the batch, but keep it on the tracked collection so
        // EF issues an UPDATE (not a hard DELETE). The projection filters it out.
        batch.IsDeleted = true;
        batch.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return VentureProjection.ToDto(venture);
    }
}
