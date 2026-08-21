using CRN_Technical_Assessment.Application.DTOs;
using FluentValidation;

namespace CRN_Technical_Assessment.Application.Validators;

/// <summary>
/// Validation rules for ProductCreateDto.
/// </summary>
public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters.")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Product name must not contain only whitespace.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required.")
            .MaximumLength(100).WithMessage("CreatedBy must not exceed 100 characters.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Item quantity must be greater than 0.");
        });
    }
}
