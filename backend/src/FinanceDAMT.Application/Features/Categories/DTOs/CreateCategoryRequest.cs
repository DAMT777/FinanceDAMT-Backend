using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Categories.DTOs;

public sealed record CreateCategoryRequest(
    string Name,
    string Icon,
    string Color,
    CategoryType Type
);
