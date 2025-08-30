using Microsoft.EntityFrameworkCore;
using OrderSystem.Application;
using OrderSystem.Application.Abstraction;
using OrderSystem.Application.Orders;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Persistence;

namespace OrderSystem.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _db;
    public OrderService(OrderDbContext db) => _db = db;

    public async Task<(bool Ok, string? Error, int? OrderId)> CreateAsync(CreateOrderDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
            return (false, "UserId zorunlu.", null);
        if (dto.Items is null || dto.Items.Count == 0)
            return (false, "En az bir satır olmalı.", null);
        if (dto.Items.Any(i => i.Quantity <= 0))
            return (false, "Adet 1 veya daha büyük olmalı.", null);

        var pids = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => pids.Contains(p.Id) && p.IsActive && !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, ct);

        if (products.Count != pids.Count)
            return (false, "Ürün bulunamadı.", null);

        foreach (var it in dto.Items)
        {
            var p = products[it.ProductId];
            if (p.Stock < it.Quantity)
                return (false, $"{p.Name} için stok yetersiz.", null);
        }

        var order = new Order { UserId = dto.UserId, CreatedAt = DateTime.UtcNow, Items = new List<OrderItem>() };
        decimal sub = 0m;

        foreach (var it in dto.Items)
        {
            var p = products[it.ProductId];
            p.Stock -= it.Quantity; // tracking + RowVersion ile korunuyor

            var line = new OrderItem
            {
                ProductId = p.Id,
                Quantity = it.Quantity,
                UnitPrice = p.Price,
                LineTotal = p.Price * it.Quantity
            };
            sub += line.LineTotal;
            order.Items.Add(line);
        }

        order.Total = sub;
        _db.Orders.Add(order);

        try
        {
            await _db.SaveChangesAsync(ct); // EF Core kendi transaction'ıyla yürütür
            return (true, null, order.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (false, "Stok başka bir işlem tarafından güncellendi. Lütfen tekrar deneyin.", null);
        }
    }

    public async Task<IReadOnlyList<object>> ListByUserAsync(string userId, CancellationToken ct)
        => await _db.Orders
            .Where(o => o.UserId == userId && o.IsActive && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id, o.CreatedAt, o.Total,
                Items = o.Items.Select(i => new { i.ProductId, i.UnitPrice, i.Quantity, i.LineTotal })
            })
            .ToListAsync(ct);

    public async Task<object?> GetAsync(int id, CancellationToken ct)
        => await _db.Orders
            .Where(o => o.Id == id && o.IsActive && !o.IsDeleted)
            .Select(o => new
            {
                o.Id, o.UserId, o.CreatedAt, o.Total,
                Items = o.Items.Select(i => new { i.ProductId, i.UnitPrice, i.Quantity, i.LineTotal })
            })
            .FirstOrDefaultAsync(ct);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, ct);
        if (order is null) return false;

        order.IsDeleted = true;
        order.IsActive = false;
        order.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
