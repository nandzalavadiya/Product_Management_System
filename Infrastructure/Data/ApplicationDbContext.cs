using CRN_Technical_Assessment.Domain.Entities;
using CRN_Technical_Assessment.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CRN_Technical_Assessment.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
