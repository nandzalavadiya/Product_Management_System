using CRN_Technical_Assessment.Application.DTOs;

namespace CRN_Technical_Assessment.Application.Interfaces;

/// <summary>
/// Service interface for product business operations.
/// </summary>
public interface IProductService
{
    Task<PaginatedResponse<ProductListDto>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
