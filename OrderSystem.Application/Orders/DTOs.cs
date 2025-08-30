namespace OrderSystem.Application.Orders;

public record CreateOrderItemDto(int ProductId, int Quantity);

public record CreateOrderDto(string UserId, List<CreateOrderItemDto> Items);