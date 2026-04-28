using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Categories.DTOs;

public sealed record UpdateCategoryRequest(
    string Name,
    string Icon,
    string Color,
    CategoryType Type
);
