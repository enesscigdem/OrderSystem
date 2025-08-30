namespace OrderSystem.Domain.BaseModels;
public interface IIntEntity : IEntity
{
    int Id { get; set; }
}