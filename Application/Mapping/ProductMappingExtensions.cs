using CRN_Technical_Assessment.Application.DTOs;
using CRN_Technical_Assessment.Domain.Entities;

namespace CRN_Technical_Assessment.Application.Mapping;

/// <summary>
/// Manual mapping extension methods between Product domain entities and DTOs.
/// </summary>
public static class ProductMappingExtensions
{
    public static ProductResponseDto ToResponseDto(this Product product) =>
        new()
        {
            Id = product.Id,
            ProductName = product.ProductName,
            CreatedBy = product.CreatedBy,
            CreatedOn = product.CreatedOn,
            ModifiedBy = product.ModifiedBy,
            ModifiedOn = product.ModifiedOn,
            Items = product.Items.Select(i => new ItemDto { Id = i.Id, Quantity = i.Quantity }).ToList()
        };

    public static ProductListDto ToListDto(this Product product) =>
        new()
        {
            Id = product.Id,
            ProductName = product.ProductName,
            CreatedBy = product.CreatedBy,
            CreatedOn = product.CreatedOn,
            ItemCount = product.Items.Count
        };

    public static Product ToEntity(this ProductCreateDto dto) =>
        new()
        {
            ProductName = dto.ProductName.Trim(),
            CreatedBy = dto.CreatedBy.Trim(),
            CreatedOn = DateTime.UtcNow,
            Items = dto.Items.Select(i => new Item { Quantity = i.Quantity }).ToList()
        };
}
