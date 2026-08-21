using CRN_Technical_Assessment.Application.Interfaces;
using CRN_Technical_Assessment.Domain.Entities;
using CRN_Technical_Assessment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRN_Technical_Assessment.Infrastructure.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly ApplicationDbContext _context;

    public ItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Item>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default) =>
        await _context.Items
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Item item, CancellationToken cancellationToken = default) =>
        await _context.Items.AddAsync(item, cancellationToken);

    public void Delete(Item item) =>
        _context.Items.Remove(item);
}
