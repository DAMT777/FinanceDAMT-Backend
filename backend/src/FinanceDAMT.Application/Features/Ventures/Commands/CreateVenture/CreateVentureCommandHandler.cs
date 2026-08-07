using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.CreateVenture;

public sealed class CreateVentureCommandHandler : IRequestHandler<CreateVentureCommand, VentureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateVentureCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VentureDto> Handle(CreateVentureCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var venture = new Venture
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Icon = request.Icon.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true
        };

        _context.Ventures.Add(venture);
        await _context.SaveChangesAsync(cancellationToken);

        return VentureProjection.ToDto(venture);
    }
}
