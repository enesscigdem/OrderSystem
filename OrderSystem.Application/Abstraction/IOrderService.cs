using OrderSystem.Application.Orders;

namespace OrderSystem.Application.Abstraction;

public interface IOrderService
{
    Task<(bool Ok, string? Error, int? OrderId)> CreateAsync(CreateOrderDto dto, CancellationToken ct);
    Task<IReadOnlyList<object>> ListByUserAsync(string userId, CancellationToken ct);
    Task<object?> GetAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}