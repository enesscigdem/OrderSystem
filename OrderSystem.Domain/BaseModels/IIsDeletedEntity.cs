namespace OrderSystem.Domain.BaseModels;

public interface IIsDeletedEntity
{
    bool IsDeleted { get; set; }
}