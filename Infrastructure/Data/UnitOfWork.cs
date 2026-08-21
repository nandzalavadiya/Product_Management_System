using CRN_Technical_Assessment.Application.Interfaces;
using CRN_Technical_Assessment.Infrastructure.Data.Repositories;

namespace CRN_Technical_Assessment.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IProductRepository? _products;
    private IItemRepository? _items;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public IItemRepository Items =>
        _items ??= new ItemRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() =>
        _context.Dispose();
}
