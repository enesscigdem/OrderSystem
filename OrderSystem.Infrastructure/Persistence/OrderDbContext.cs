using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Product>   Products   => Set<Product>();
    public DbSet<Order>     Orders     => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.Name);
        });

        b.Entity<Order>(e =>
        {
            e.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.OrderId);
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
        });

        b.Entity<OrderItem>(e =>
        {
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        });
    }
}