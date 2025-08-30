namespace OrderSystem.Application.Products;

public record ProductCreateDto(string Name, decimal Price, int Stock, bool IsActive = true);

public record ProductUpdateDto(string Name, decimal Price, bool IsActive, byte[] RowVersion);

public record ProductAdjustStockDto(int Delta, byte[] RowVersion);

public record ProductVm(int Id, string Name, decimal Price, int Stock, bool IsActive, byte[] RowVersion);