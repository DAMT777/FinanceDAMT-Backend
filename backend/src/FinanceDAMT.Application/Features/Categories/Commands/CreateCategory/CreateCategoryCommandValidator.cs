using FluentValidation;

namespace FinanceDAMT.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("Category icon is required.")
            .MaximumLength(50).WithMessage("Category icon must not exceed 50 characters.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Category color is required.")
            .MaximumLength(20).WithMessage("Category color must not exceed 20 characters.");
    }
}
