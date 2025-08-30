using OrderSystem.Domain.BaseModels;

namespace OrderSystem.Domain.Entities;

public class Product : IIntEntity, IAuditEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public bool IsDeleted { get; set; } = false;
    public bool IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string?  CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string?  ModifiedBy { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}