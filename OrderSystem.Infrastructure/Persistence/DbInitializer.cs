using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OrderSystem.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task MigrateAndSeedAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        await db.Database.MigrateAsync();

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                new Domain.Entities.Product { Name = "Laptop",  Price = 25000m, Stock = 5,  IsActive = true, CreatedAt = DateTime.UtcNow },
                new Domain.Entities.Product { Name = "Telefon", Price = 15000m, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Domain.Entities.Product { Name = "Kulaklık",Price = 1200m,  Stock = 50, IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
        }
    }
}