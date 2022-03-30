namespace Reusable.CRUD.Contract;

public interface IEntity
{
    long Id { get; set; }
    string EntityName { get; }
}
