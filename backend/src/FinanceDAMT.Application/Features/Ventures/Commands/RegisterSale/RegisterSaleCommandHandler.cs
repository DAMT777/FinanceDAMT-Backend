using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Ventures.Commands.RegisterSale;

public sealed class RegisterSaleCommandHandler : IRequestHandler<RegisterSaleCommand, VentureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RegisterSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VentureDto> Handle(RegisterSaleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = await _context.Ventures
            .Include(v => v.Batches)
            .FirstOrDefaultAsync(v => v.Id == request.VentureId && v.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Venture not found.");

        var batch = venture.Batches.FirstOrDefault(b => b.Id == request.BatchId)
            ?? throw new NotFoundException("Batch not found.");

        var remaining = batch.UnitsProduced - batch.UnitsSold;
        if (request.Units > remaining)
            throw new ConflictException($"Only {remaining} unit(s) available to sell.");

        batch.UnitsSold += request.Units;

        await _context.SaveChangesAsync(cancellationToken);

        return VentureProjection.ToDto(venture);
    }
}
