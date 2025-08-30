using OrderSystem.Application.Products;

namespace OrderSystem.Application;

public interface IProductService
{
    Task<(bool Ok, string? Error, int? Id)> CreateAsync(ProductCreateDto dto, CancellationToken ct);
    Task<(bool Ok, string? Error)> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct);
    Task<(bool Ok, string? Error)> AdjustStockAsync(int id, ProductAdjustStockDto dto, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<ProductVm?> GetAsync(int id, CancellationToken ct);

    Task<(IReadOnlyList<ProductVm> Items, int TotalCount)> ListAsync(
        string? q, bool? onlyActive, int page, int pageSize, CancellationToken ct);
}