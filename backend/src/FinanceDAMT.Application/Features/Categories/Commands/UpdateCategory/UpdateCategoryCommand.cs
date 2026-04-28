using FinanceDAMT.Application.Features.Categories.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Icon,
    string Color,
    CategoryType Type
) : IRequest<CategoryDto>;
