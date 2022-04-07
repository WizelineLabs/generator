namespace Reusable.CRUD.Implementations.EF;

using Microsoft.EntityFrameworkCore;
using Reusable.CRUD.Contract;
using Reusable.Rest;
using ServiceStack.Text;
using System.Reflection;

public abstract class ReadOnlyLogic<Entity> : BaseLogic, ILogicReadOnly<Entity>, ILogicReadOnlyAsync<Entity> where Entity : class, IEntity, new()
{
    protected static Entity EntityInfo = new Entity();

    protected ReadOnlyLogic(DbContext DbContext) : base(DbContext)
    {
    }

    protected static string CACHE_GET_ALL(object? entityInfo = null) => "_GET_ALL_" + (entityInfo == null ? typeof(Entity).Name : entityInfo.GetType().Name);
    protected static string CACHE_GET_BY_ID(object? entityInfo = null) => "_GET_BY_ID_" + (entityInfo == null ? typeof(Entity).Name : entityInfo.GetType().Name) + "_";
    protected static string CACHE_GET_PAGED(object? entityInfo = null) => "_QUERY_" + (entityInfo == null ? typeof(Entity).Name : entityInfo.GetType().Name) + "_";
    protected static string CACHE_CUSTOM(object? entityInfo = null) => "_CUSTOM_" + (entityInfo == null ? typeof(Entity).Name : entityInfo.GetType().Name) + "_";

    public string GenerateCacheKey(object from, bool toMD5 = true)
    {
        if (toMD5)
            return CACHE_CUSTOM() + ToMD5(from);
        else
            return CACHE_CUSTOM() + from?.ToString()?.ToUpper();
    }

    public string GenerateCacheKey(string id, object toMD5)
    {
        if (toMD5 != null)
        {
            if (toMD5 is List<string> listSource)
            {
                return CACHE_CUSTOM() + id.ToUpper() + "_" + ToMD5(listSource.Distinct().OrderBy(s => s).ToList());
            }
            return CACHE_CUSTOM() + id.ToUpper() + "_" + ToMD5(toMD5);
        }
        else
            return CACHE_CUSTOM() + id.ToUpper();
    }

    public string GenerateCacheKey(string id)
    {
        return CACHE_CUSTOM() + id.ToUpper();
    }

    #region HOOKS
    virtual protected SqlExpression<Entity> OnGetList(SqlExpression<Entity> query)
    {
        if (EntityInfo is Trackable) query.Where($"{query.Table<Entity>()}.{query.Column<Trackable>(t => t.IsDeleted)} = FALSE");
        return query;
    }
    virtual protected SqlExpression<Entity> OnGetSingle(SqlExpression<Entity> query)
    {
        return OnGetList(query);
    }
    virtual public List<Entity> AdapterOut(params Entity[] entities) { return entities.ToList(); }
    virtual protected List<Entity> BeforePaginate(List<Entity> entities) { return entities; }
    protected delegate List<Entity> BeforeSearchHandler(List<Entity> entities);
    protected BeforeSearchHandler? BeforeSearch { get; set; }
    #endregion

    virtual public List<Entity> GetAll()
    {
        var cache = CacheDisabled ? null : Cache?.Get<List<Entity>>(CACHE_GET_ALL());
        if (cache != null)
        {
            if (Log.IsDebugEnabled)
                Log.Info($"Cache. Get All of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}].");

            return cache;
        }

        // var query = Db.From<Entity>();
        // var entities = Db.LoadSelect(query);

        if (Log.IsDebugEnabled)
            Log.Info($"SQL. Get All of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}].");

        // if (!CacheDisabled) Cache?.Set(CACHE_GET_ALL(), entities);
        // return entities;
        return new List<Entity>(); // TODO: return entries from db
    }

    virtual public async Task<List<Entity>> GetAllAsync()
    {
        var cache = CacheDisabled ? null : Cache?.Get<List<Entity>>(CACHE_GET_ALL());
        if (cache != null)
        {
            if (Log.IsDebugEnabled)
                Log.Info($"Cache. Get All Async of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}].");

            return cache;
        }

        // var query = Db.From<Entity>();
        // var entities = await Db.LoadSelectAsync(query);

        if (Log.IsDebugEnabled)
            Log.Info($"SQL. Get All Async of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}]");

        // if (!CacheDisabled) Cache?.Set(CACHE_GET_ALL(), entities);
        // return entities;
        return new List<Entity>(); // TODO: return entries from db
    }

    virtual public Entity? GetById(long id)
    {
        var cacheKey = CACHE_GET_BY_ID() + id;
        var cache = CacheDisabled ? null : Cache?.Get<Entity>(cacheKey);
        if (cache != null)
        {
            if (Log.IsDebugEnabled)
                Log.Info($"Cache. Get By Id: [{id}] of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}].");

            return AdapterOut(cache)[0];
        }

        // var query = OnGetSingle(Db.From<Entity>())
        //         .Where(e => e.Id == id);

        // var entity = Db.LoadSelect(query).FirstOrDefault();

        // if (entity != null) AdapterOut(entity);

        // if (Log.IsDebugEnabled)
        //     Log.Info($"SQL. Get By Id: [{id}] of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}].");

        // var response = entity;
        // if (!CacheDisabled) Cache?.Set(cacheKey, response);
        // return response;
        return new Entity(); // TODO: return entity from db
    }

    virtual public async Task<Entity?> GetByIdAsync(long id)
    {
        var cacheKey = CACHE_GET_BY_ID() + id;
        var cache = CacheDisabled ? null : Cache?.Get<Entity>(cacheKey);
        if (cache != null)
        {
            if (Log.IsDebugEnabled)
                Log.Info($"Cache. Get By Id Async: [{id}] of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}].");

            return AdapterOut(cache)[0];
        }

        // var query = OnGetSingle(Db.From<Entity>())
        //         .Where(e => e.Id == id);

        // var entity = (await Db.LoadSelectAsync(query)).FirstOrDefault();

        // if (entity != null) AdapterOut(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"SQL. Get By Id Async: [{id}] of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}].");

        // var response = entity;
        // if (!CacheDisabled) Cache?.Set(cacheKey, response);
        // return response;
        return new Entity(); // TODO: return entity from db
    }

    private (bool, string, CommonResponse?, SqlExpression<Entity>) TryGetPagedFromCache(int perPage, int page, string generalFilter, SqlExpression<Entity>? query, string? cacheKey, bool requiresKeysInJsons, bool useOnGetList)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) cacheKey = "";
        cacheKey = $"{CACHE_GET_PAGED()}_{generalFilter}_{cacheKey}";
        if (requiresKeysInJsons) cacheKey += "_requiresKeysInJsons";

        // var allParams = Request.GetRequestParams();
        // foreach (var param in allParams)
        // {
        //     if (IsValidJSValue(param.Value) && IsValidCacheParam(param.Key))
        //         cacheKey += $"_{param.Key}_{param.Value}";
        // }

        // if (query == null) query = Db.From<Entity>();

        // var cache = CacheDisabled ? null : Cache?.Get<CommonResponse>(cacheKey);
        // if (cache != null)
        // {
        //     if (Log.IsDebugEnabled)
        //         Log.Info($"Cache. Get Paged ({(cache.AdditionalData as FilterResponse)?.total_filtered_items}): [{cacheKey}] of: [{EntityInfo.EntityName}] by: [{Auth?.UserName}].");

        //     cache.Result = cache.Result?.ConvertTo<List<Entity>>() ?? new List<Entity>();
        //     return (true, cacheKey, cache, query);
        // }

        // if (useOnGetList)
        //     query = OnGetList(query);

        // return (false, cacheKey, null, query);
        return (false, cacheKey, null, null); // TODO: Implement TryGetPagedFromCache correctly
    }

    private HashSet<Entity> ApplyGeneralFilter(List<Entity> entities, string generalFilter)
    {
        if (BeforeSearch != null)
            entities = BeforeSearch(entities);

        var filtered = new HashSet<Entity>();
        if (!string.IsNullOrEmpty(generalFilter)) // || (paramsNotFoundAsProps.Count > 0))
        {
            var searchableProps = typeof(Entity).GetProperties().Where(prop => !prop.HasAttribute<IsJson>()
                                && new[] { "String" }.Contains(prop.PropertyType.Name)).ToList();
            // var jsonProps = typeof(Entity).GetPublicProperties().Where(p => p.HasAttribute<IsJson>()).ToList();

            foreach (var entity in entities)
                if (SearchInStringProps(entity, generalFilter))
                    // && SearchInJsonProps(entity, paramsNotFoundAsProps, requiresKeysInJsons, jsonProps))
                    filtered.Add(entity);
        }
        else
            filtered = new HashSet<Entity>(entities);

        return filtered;
    }

    private CommonResponse ApplyPagination(int perPage, int page, HashSet<Entity> filtered, FilterResponse filterResponse, string cacheKey)
    {
        IEnumerable<Entity> afterPaginate;
        if (perPage != 0)
        {
            var totalPagesCount = (filterResponse.total_items + perPage - 1) / perPage;
            if (page > totalPagesCount)
                page = totalPagesCount;

            afterPaginate = BeforePaginate(filtered.ToList()).Skip((page - 1) * perPage).Take(perPage);
            filterResponse.page = page;
        }
        else
            afterPaginate = BeforePaginate(filtered.ToList());

        #region AdapterOut Hook
        if (BeforeSearch == null)
            AdapterOut(afterPaginate.ToArray());
        #endregion

        var response = new CommonResponse { Result = afterPaginate.ToList(), AdditionalData = filterResponse };

        if (Log.IsDebugEnabled)
            Log.Info($"SQL. Get Paged ({filterResponse.total_filtered_items}): [{cacheKey}] of [{EntityInfo.EntityName}] by: [{Auth?.UserName}]");

        if (!CacheDisabled) Cache?.Set(cacheKey, response);
        return response;
    }

    virtual public CommonResponse? GetPaged(int perPage = 0, int page = 1, string generalFilter = "", SqlExpression<Entity>? queryParam = null, string? cacheKeyParam = null, bool requiresKeysInJsons = false, bool useOnGetList = true)
    {
        var (cacheFound, cacheKey, responseFromCache, query) = TryGetPagedFromCache(perPage, page, generalFilter, queryParam, cacheKeyParam, requiresKeysInJsons, useOnGetList);
        if (cacheFound) return responseFromCache;

        // if (Log.IsDebugEnabled) OrmLiteUtils.PrintSql();
        // var entities = Db.LoadSelect(query);
        // if (Log.IsDebugEnabled) OrmLiteUtils.UnPrintSql();

        // var filterResponse = new FilterResponse
        // {
        //     total_items = (int)Db.Count<Entity>(query)
        // };

        // var filtered = ApplyGeneralFilter(entities, generalFilter);
        // filterResponse.total_filtered_items = filtered.Count();

        // return ApplyPagination(perPage, page, filtered, filterResponse, cacheKey);
        return ApplyPagination(perPage, page, new HashSet<Entity>(), null, cacheKey); // TODO: implementation
    }

    virtual public async Task<CommonResponse?> GetPagedAsync(int perPage = 0, int page = 1, string generalFilter = "", SqlExpression<Entity>? queryParam = null, string? cacheKeyParam = null, bool requiresKeysInJsons = false, bool useOnGetList = true)
    {
        var (cacheFound, cacheKey, responseFromCache, query) = TryGetPagedFromCache(perPage, page, generalFilter, queryParam, cacheKeyParam, requiresKeysInJsons, useOnGetList);
        if (cacheFound) return responseFromCache;

        if (Log.IsDebugEnabled)
            Log.Info("GetPagedAsync");

        // if (Log.IsDebugEnabled) OrmLiteUtils.PrintSql();
        // var entities = await Db.LoadSelectAsync(query);
        // if (Log.IsDebugEnabled) OrmLiteUtils.UnPrintSql();

        // var filterResponse = new FilterResponse
        // {
        //     total_items = (int)(await Db.CountAsync<Entity>(query))
        // };

        // var filtered = ApplyGeneralFilter(entities, generalFilter);
        // filterResponse.total_filtered_items = filtered.Count();

        // return ApplyPagination(perPage, page, filtered, filterResponse, cacheKey);
        return ApplyPagination(perPage, page, new HashSet<Entity>(), null, cacheKey); // TODO: implementation
    }

    virtual public Entity? GetSingleWhere(string Property, object Value, SqlExpression<Entity>? query = null, string? cacheKey = null, bool useOnGetList = true)
    {
        var getPagedResponse = GetPaged(1, 1, "", null, $"GetSingleWhere_Property_{Property}_Value_{Value}", useOnGetList: useOnGetList);
        var result = getPagedResponse?.Result as List<Entity>;
        if (result?.Count > 0)
            return result[0];

        return null;
    }

    virtual public async Task<Entity?> GetSingleWhereAsync(string Property, object Value, SqlExpression<Entity>? query = null, string? cacheKey = null, bool useOnGetList = true)
    {
        var getPagedResponse = await GetPagedAsync(1, 1, "", null, $"GetSingleWhere_Property_{Property}_Value_{Value}", useOnGetList: useOnGetList);
        var result = getPagedResponse?.Result as List<Entity>;
        if (result?.Count > 0)
            return result[0];

        return null;
    }

    public bool SearchInStringProps(Entity entity, string criteria = "", List<PropertyInfo>? searchableProps = null)
    {
        if (string.IsNullOrWhiteSpace(criteria)) return true;

        if (Log.IsDebugEnabled)
            Log.Info("Search in String Props.");

        var splitGeneralFilter = criteria.ToLower().Split(' ').Select(e => e.Trim()).ToList();
        searchableProps = searchableProps ?? typeof(Entity).GetProperties().Where(prop => !prop.HasAttribute<IsJson>()
                                && new[] { "String" }.Contains(prop.PropertyType.Name)).ToList();

        foreach (var keyword in splitGeneralFilter)
        {
            bool bAtLeastOnePropertyContainsIt = false;
            foreach (var prop in searchableProps)
            {
                string? value = (string?)prop?.GetValue(entity, null);
                if (value != null && value.ToLower().Contains(keyword))
                {
                    bAtLeastOnePropertyContainsIt = true;
                    break;
                }
            }

            if (!bAtLeastOnePropertyContainsIt)
                return false;
        }

        return true;
    }

    public bool SearchInJsonProps(Entity entity, Dictionary<string, string> properties, bool keysAreRequired = false, List<PropertyInfo>? jsonProps = null)
    {
        jsonProps = jsonProps ?? typeof(Entity).GetPublicProperties().Where(p => p.HasAttribute<IsJson>()).ToList();
        if (jsonProps.Count == 0 || properties == null || properties.Count == 0) return true;

        if (Log.IsDebugEnabled)
            Log.Info("Search in Json Props.");

        foreach (var prop in properties)
            foreach (var jsonProp in jsonProps)
            {
                var jsonValue = JsonObject.Parse(jsonProp.GetValue(entity) as string);
                if (jsonValue != null && jsonValue.ContainsKey(prop.Key))
                {
                    //As value
                    string value = jsonValue[prop.Key];
                    if (value != null)
                    {
                        if (value != null && value.Trim().ToLower().Contains(prop.Value.Trim().ToLower()))
                            return true;
                    }
                    else
                    {
                        //As array
                        var splitValue = prop.Value.Split(',');
                        var list = jsonValue.ArrayObjects(prop.Key);
                        var found = list.Any(e => splitValue.Any(s => e["Value"].Trim().ToLower().Contains(s.Trim().ToLower())));
                        if (found != false)
                            return true;
                    }

                    return false;
                }
                else if (keysAreRequired)
                    return false;
            }

        return true;
    }
}
