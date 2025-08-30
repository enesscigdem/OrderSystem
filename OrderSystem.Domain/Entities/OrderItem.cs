using OrderSystem.Domain.BaseModels;

namespace OrderSystem.Domain.Entities;

public class OrderItem : IIntEntity, IAuditEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public bool IsDeleted { get; set; } = false;
    public bool IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string?  CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string?  ModifiedBy { get; set; }
}