using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Ventures.Commands.AddBatch;

public sealed class AddBatchCommandHandler : IRequestHandler<AddBatchCommand, VentureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddBatchCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VentureDto> Handle(AddBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = await _context.Ventures
            .Include(v => v.Batches)
            .FirstOrDefaultAsync(v => v.Id == request.VentureId && v.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Venture not found.");

        // Add through the DbSet (not venture.Batches) so EF marks it Added and
        // emits an INSERT. Adding to the tracked parent's collection makes EF
        // treat the pre-populated Guid key as an existing row and emit an UPDATE
        // (0 rows affected -> DbUpdateConcurrencyException). Relationship fixup
        // still adds it to venture.Batches for the projection below.
        _context.VentureBatches.Add(new VentureBatch
        {
            VentureId = venture.Id,
            Label = request.Label.Trim(),
            Date = request.Date,
            Investment = request.Investment,
            UnitsProduced = request.UnitsProduced,
            UnitPrice = request.UnitPrice,
            UnitsSold = 0,
            Notes = request.Notes?.Trim()
        });

        await _context.SaveChangesAsync(cancellationToken);

        return VentureProjection.ToDto(venture);
    }
}
