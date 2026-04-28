using FinanceDAMT.Application.Features.Categories.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
