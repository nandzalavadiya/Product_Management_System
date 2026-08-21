using CRN_Technical_Assessment.Domain.Entities;

namespace CRN_Technical_Assessment.Application.Interfaces;

/// <summary>
/// Repository interface for Item data access operations.
/// </summary>
public interface IItemRepository
{
    Task<IEnumerable<Item>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task AddAsync(Item item, CancellationToken cancellationToken = default);
    void Delete(Item item);
}
