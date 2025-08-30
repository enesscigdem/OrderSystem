using Microsoft.EntityFrameworkCore;
using OrderSystem.Application;
using OrderSystem.Application.Products;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Persistence;

namespace OrderSystem.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly OrderDbContext _db;
    public ProductService(OrderDbContext db) => _db = db;

    public async Task<(bool Ok, string? Error, int? Id)> CreateAsync(ProductCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return (false, "İsim zorunlu.", null);
        if (dto.Price < 0 || dto.Stock < 0) return (false, "Fiyat/Stock negatif olamaz.", null);

        var exists = await _db.Products.AnyAsync(p => p.Name == dto.Name && !p.IsDeleted, ct);
        if (exists) return (false, "Bu isimde ürün zaten var.", null);

        var p = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(p);
        await _db.SaveChangesAsync(ct);
        return (true, null, p.Id);
    }

    public async Task<(bool Ok, string? Error)> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (p is null) return (false, "Ürün bulunamadı.");

        p.Name = dto.Name;
        p.Price = dto.Price;
        p.IsActive = dto.IsActive;
        p.ModifiedAt = DateTime.UtcNow;

        _db.Entry(p).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await _db.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (false, "Ürün başka biri tarafından güncellendi.");
        }
    }

    public async Task<(bool Ok, string? Error)> AdjustStockAsync(int id, ProductAdjustStockDto dto,
        CancellationToken ct)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (p is null) return (false, "Ürün bulunamadı.");

        var newStock = p.Stock + dto.Delta;
        if (newStock < 0) return (false, "Stok negatif olamaz.");

        p.Stock = newStock;
        p.ModifiedAt = DateTime.UtcNow;

        _db.Entry(p).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await _db.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (false, "Stok başka biri tarafından değişti.");
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (p is null) return false;

        p.IsDeleted = true;
        p.IsActive = false;
        p.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ProductVm?> GetAsync(int id, CancellationToken ct)
        => await _db.Products
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new ProductVm(p.Id, p.Name, p.Price, p.Stock, p.IsActive, p.RowVersion))
            .FirstOrDefaultAsync(ct);

    public async Task<(IReadOnlyList<ProductVm> Items, int TotalCount)> ListAsync(
        string? q, bool? onlyActive, int page, int pageSize, CancellationToken ct)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        var query = _db.Products.AsNoTracking().Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q));

        if (onlyActive == true)
            query = query.Where(p => p.IsActive);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductVm(p.Id, p.Name, p.Price, p.Stock, p.IsActive, p.RowVersion))
            .ToListAsync(ct);

        return (items, total);
    }
}