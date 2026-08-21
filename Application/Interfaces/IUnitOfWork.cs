namespace CRN_Technical_Assessment.Application.Interfaces;

/// <summary>
/// Unit of Work — coordinates multiple repositories under a single database transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IItemRepository Items { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
