using OrderSystem.Domain.BaseModels;

namespace OrderSystem.Domain.Entities;

public class Order : IIntEntity, IAuditEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public decimal Total { get; set; }

    public bool IsDeleted { get; set; } = false;
    public bool IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string?  CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string?  ModifiedBy { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}