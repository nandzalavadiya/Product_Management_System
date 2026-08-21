using CRN_Technical_Assessment.Application.Validators;
using CRN_Technical_Assessment.Application.DTOs;
using FluentValidation.TestHelper;

namespace CRN_Technical_Assessment.Tests.Validators;

public class ProductValidatorTests
{
    private readonly ProductCreateDtoValidator _validator = new();

    [Fact]
    public void ProductCreateDto_ValidInput_PassesValidation()
    {
        var dto = new ProductCreateDto
        {
            ProductName = "Valid Product",
            CreatedBy = "admin"
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ProductCreateDto_EmptyProductName_FailsValidation()
    {
        var dto = new ProductCreateDto { ProductName = "", CreatedBy = "admin" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void ProductCreateDto_ProductNameTooShort_FailsValidation()
    {
        var dto = new ProductCreateDto { ProductName = "A", CreatedBy = "admin" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void ProductCreateDto_ProductNameTooLong_FailsValidation()
    {
        var dto = new ProductCreateDto { ProductName = new string('A', 256), CreatedBy = "admin" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void ProductCreateDto_WhitespaceOnlyProductName_FailsValidation()
    {
        var dto = new ProductCreateDto { ProductName = "   ", CreatedBy = "admin" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void ProductCreateDto_EmptyCreatedBy_FailsValidation()
    {
        var dto = new ProductCreateDto { ProductName = "Valid Product", CreatedBy = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy);
    }

    [Fact]
    public void ProductCreateDto_CreatedByTooLong_FailsValidation()
    {
        var dto = new ProductCreateDto { ProductName = "Valid Product", CreatedBy = new string('A', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy);
    }

    [Fact]
    public void ProductCreateDto_ItemWithZeroQuantity_FailsValidation()
    {
        var dto = new ProductCreateDto
        {
            ProductName = "Valid Product",
            CreatedBy = "admin",
            Items = new List<ItemDto> { new() { Quantity = 0 } }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }
}
