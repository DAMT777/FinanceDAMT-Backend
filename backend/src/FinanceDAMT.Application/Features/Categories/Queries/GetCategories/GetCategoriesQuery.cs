using FinanceDAMT.Application.Features.Categories.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
