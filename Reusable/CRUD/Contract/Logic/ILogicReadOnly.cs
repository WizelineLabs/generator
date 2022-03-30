namespace Reusable.CRUD.Contract;

using Reusable.Rest;

public interface ILogicReadOnly<Entity> : ILogic where Entity : class, new()
{
    List<Entity> GetAll();
    Entity? GetById(long Id);
    CommonResponse? GetPaged(int perPage, int page, string search, SqlExpression<Entity>? query = null, string? cacheKey = null, bool requiresKeysInJson = false, bool useOnGetList = true);
    Entity? GetSingleWhere(string Property, object Value, SqlExpression<Entity>? query = null, string? cacheKey = null, bool useOnGetList = true);

    Exception GetOriginalException(Exception ex);
}

public interface ILogicReadOnlyAsync<Entity> : ILogic where Entity : class, new()
{
    Task<List<Entity>> GetAllAsync();
    Task<Entity?> GetByIdAsync(long id);
    Task<CommonResponse?> GetPagedAsync(int perPage, int page, string search, SqlExpression<Entity>? query = null, string? cacheKey = null, bool requiresKeysInJson = false, bool useOnGetList = true);
    Task<Entity?> GetSingleWhereAsync(string Property, object Value, SqlExpression<Entity>? query = null, string? cacheKey = null, bool useOnGetList = true);
}
