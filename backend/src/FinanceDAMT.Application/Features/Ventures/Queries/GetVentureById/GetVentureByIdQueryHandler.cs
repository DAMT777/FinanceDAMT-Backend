using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Ventures.Queries.GetVentureById;

public sealed class GetVentureByIdQueryHandler : IRequestHandler<GetVentureByIdQuery, VentureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetVentureByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VentureDto> Handle(GetVentureByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = await _context.Ventures
            .AsNoTracking()
            .Include(v => v.Batches)
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Venture not found.");

        return VentureProjection.ToDto(venture);
    }
}
