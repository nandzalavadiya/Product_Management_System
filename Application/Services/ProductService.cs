using CRN_Technical_Assessment.Application.DTOs;
using CRN_Technical_Assessment.Application.Interfaces;
using CRN_Technical_Assessment.Application.Mapping;
using CRN_Technical_Assessment.Domain.Entities;
using CRN_Technical_Assessment.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CRN_Technical_Assessment.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProductListDto>> GetAllPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching products - page {PageNumber}, size {PageSize}", pageNumber, pageSize);

        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        var dtos = items.Select(p => p.ToListDto());

        return PaginatedResponse<ProductListDto>.Ok(dtos, pageNumber, pageSize, totalCount, "Products retrieved successfully.");
    }

    public async Task<ProductResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching product with Id {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        return product.ToResponseDto();
    }

    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product '{ProductName}' by '{CreatedBy}'", dto.ProductName, dto.CreatedBy);

        var product = dto.ToEntity();

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created with Id {ProductId}", product.Id);

        return product.ToResponseDto();
    }

    public async Task<ProductResponseDto> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with Id {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.ProductName = dto.ProductName.Trim();
        product.ModifiedBy = dto.ModifiedBy.Trim();
        product.ModifiedOn = DateTime.UtcNow;

        product.Items.Clear();
        foreach (var itemDto in dto.Items)
        {
            product.Items.Add(new Item { Quantity = itemDto.Quantity });
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product with Id {ProductId} updated successfully", id);

        return product.ToResponseDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with Id {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product with Id {ProductId} deleted successfully", id);
    }
}
