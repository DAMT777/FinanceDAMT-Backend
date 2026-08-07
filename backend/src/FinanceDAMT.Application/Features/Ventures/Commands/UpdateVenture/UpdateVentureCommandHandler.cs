using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Ventures.Commands.UpdateVenture;

public sealed class UpdateVentureCommandHandler : IRequestHandler<UpdateVentureCommand, VentureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateVentureCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VentureDto> Handle(UpdateVentureCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = await _context.Ventures
            .Include(v => v.Batches)
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Venture not found.");

        venture.Name = request.Name.Trim();
        venture.Icon = request.Icon.Trim();
        venture.Description = request.Description?.Trim();
        venture.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return VentureProjection.ToDto(venture);
    }
}
