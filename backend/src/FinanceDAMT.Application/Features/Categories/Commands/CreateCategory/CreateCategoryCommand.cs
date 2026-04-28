using FinanceDAMT.Application.Features.Categories.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string Icon,
    string Color,
    CategoryType Type
) : IRequest<CategoryDto>;
