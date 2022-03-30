namespace Reusable.CRUD.Contract;

public interface ILogicWrite<Entity> : ILogicReadOnly<Entity> where Entity : class, new()
{
    Entity CreateInstance(Entity? entity = null);
    Entity Add(Entity entity);
    Entity Update(Entity entity);
    void Remove(Entity id);
    void RemoveAll();
}

public interface ILogicWriteAsync<Entity> : ILogicReadOnlyAsync<Entity> where Entity : class, new()
{
    Task<Entity> AddAsync(Entity entity);
    Task<Entity> UpdateAsync(Entity entity);
    Task RemoveAsync(Entity entity);
}
