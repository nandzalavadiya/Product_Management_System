using CRN_Technical_Assessment.Domain.Entities;
using CRN_Technical_Assessment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRN_Technical_Assessment.Extensions;


public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration applied successfully.");

            await SeedUsersAsync(context, logger);
            await SeedProductsAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync()) return;

        logger.LogInformation("Seeding users...");

        var users = new List<User>
        {
            new()
            {
                Username = "admin",
                Email = "admin@crn.local",
                // Development credentials only — never commit real credentials
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                Username = "user1",
                Email = "user1@crn.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = "User",
                CreatedOn = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} users.", users.Count);
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Products.AnyAsync()) return;

        logger.LogInformation("Seeding products...");

        var products = new List<Product>
        {
            new()
            {
                ProductName = "Laptop Pro X1",
                CreatedBy = "admin",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 50 },
                    new() { Quantity = 25 }
                }
            },
            new()
            {
                ProductName = "Wireless Mouse M200",
                CreatedBy = "admin",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 200 }
                }
            },
            new()
            {
                ProductName = "Mechanical Keyboard K500",
                CreatedBy = "admin",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 75 },
                    new() { Quantity = 30 }
                }
            },
            new()
            {
                ProductName = "USB-C Hub 7-in-1",
                CreatedBy = "admin",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 150 }
                }
            },
            new()
            {
                ProductName = "4K Monitor UltraWide",
                CreatedBy = "admin",
                CreatedOn = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new() { Quantity = 40 },
                    new() { Quantity = 10 }
                }
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} products.", products.Count);
    }
}
