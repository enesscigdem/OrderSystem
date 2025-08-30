namespace OrderSystem.Domain.BaseModels;

public interface IActivateableEntity
{
    bool IsActive { get; set; }
}