using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Categories.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var duplicate = await _context.Categories.AnyAsync(
            c => c.UserId == userId && c.Name == request.Name && c.Type == request.Type,
            cancellationToken);

        if (duplicate)
            throw new ConflictException("A category with the same name already exists.");

        var category = new Category
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Icon = request.Icon.Trim(),
            Color = request.Color.Trim(),
            Type = request.Type
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Icon, category.Color, category.Type, IsGlobal: false);
    }
}
