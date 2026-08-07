using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Ventures.Commands.UpdateBatch;

public sealed class UpdateBatchCommandHandler : IRequestHandler<UpdateBatchCommand, VentureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateBatchCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VentureDto> Handle(UpdateBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = await _context.Ventures
            .Include(v => v.Batches)
            .FirstOrDefaultAsync(v => v.Id == request.VentureId && v.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Venture not found.");

        var batch = venture.Batches.FirstOrDefault(b => b.Id == request.BatchId)
            ?? throw new NotFoundException("Batch not found.");

        batch.Label = request.Label.Trim();
        batch.Date = request.Date;
        batch.Investment = request.Investment;
        batch.UnitsProduced = request.UnitsProduced;
        batch.UnitPrice = request.UnitPrice;
        // Never leave more units sold than produced after an edit.
        if (batch.UnitsSold > batch.UnitsProduced)
            batch.UnitsSold = batch.UnitsProduced;
        batch.Notes = request.Notes?.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return VentureProjection.ToDto(venture);
    }
}
