namespace Reusable.CRUD.Implementations.EF;

using Microsoft.EntityFrameworkCore;
using Reusable.CRUD.Contract;
using Reusable.Rest;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class WriteLogic<Entity> : ReadOnlyLogic<Entity>, ILogicWrite<Entity>, ILogicWriteAsync<Entity> where Entity : class, IEntity, new()
{
    public WriteLogic(DbContext DbContext) : base(DbContext)
    {
    }
    #region HOOKS
    virtual protected Entity OnCreateInstance(Entity entity) { return entity; }
    virtual protected void OnAfterSaving(Entity entity, OPERATION_MODE mode = OPERATION_MODE.NONE) { }
    virtual protected void OnBeforeSaving(Entity entity, OPERATION_MODE mode = OPERATION_MODE.NONE) { }
    virtual protected void OnBeforeRemoving(Entity entity) { }
    virtual protected void OnBeforeRemoving(long id) { }
    #endregion

    virtual public Entity CreateInstance(Entity? entity = null)
    {
        if (entity == null) entity = new Entity();

        if (Log.IsDebugEnabled)
            Log.Info($"Create Instance of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");

        return OnCreateInstance(entity);
    }

    virtual public Entity Add(Entity entity)
    {
        OnBeforeSaving(entity, OPERATION_MODE.ADD);
        if (entity is Trackable track)
        {
            track.CreatedAt = DateTimeOffset.Now;
            track.CreatedBy = Auth?.UserName;
        }
        // entity.Id = Db.Insert(entity, selectIdentity: true);

        OnAfterSaving(entity, OPERATION_MODE.ADD);

        CacheOnAdd(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Inserted Entity [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");

        return entity;
    }

    virtual public async Task<Entity> AddAsync(Entity entity)
    {
        OnBeforeSaving(entity, OPERATION_MODE.ADD);
        if (entity is Trackable track)
        {
            track.CreatedAt = DateTimeOffset.Now;
            track.CreatedBy = Auth?.UserName;
        }
        // entity.Id = await Db.InsertAsync(entity, selectIdentity: true);

        OnAfterSaving(entity, OPERATION_MODE.ADD);

        CacheOnAdd(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Inserted Entity Async [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");

        return entity;
    }

    virtual public Entity Update(Entity entity)
    {
        OnBeforeSaving(entity, OPERATION_MODE.UPDATE);
        if (entity is Trackable track)
        {
            track.UpdatedAt = DateTimeOffset.Now;
            track.UpdatedBy = Auth?.UserName;
        }
        // Db.Update(entity);

        OnAfterSaving(entity, OPERATION_MODE.UPDATE);

        CacheOnUpdate(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Updated Entity [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");

        return entity;
    }

    virtual public async Task<Entity> UpdateAsync(Entity entity)
    {
        OnBeforeSaving(entity, OPERATION_MODE.UPDATE);
        if (entity is Trackable track)
        {
            track.UpdatedAt = DateTimeOffset.Now;
            track.UpdatedBy = Auth?.UserName;
        }
        // await Db.UpdateAsync(entity);

        OnAfterSaving(entity, OPERATION_MODE.UPDATE);

        CacheOnUpdate(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Updated Entity Async [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");

        return entity;
    }

    virtual public void RemoveById(long id)
    {
        var entity = GetById(id);
        if (entity == null) throw new KnownError("Error. Cannot remove entity, it no longer exists");

        Remove(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Removed Entity by Id [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");
    }

    virtual public async Task RemoveByIdAsync(long id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) throw new KnownError("Error. Cannot remove entity, it no longer exists");

        await RemoveAsync(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Removed Entity by Id Async [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");
    }

    virtual public void Remove(Entity entity)
    {
        OnBeforeRemoving(entity);
        if (entity is Trackable track)
        {
            track.RemovedAt = DateTimeOffset.Now;
            track.RemovedBy = Auth?.UserName;
            // Db.Update<Entity>(new { IsDeleted = true, track.RemovedAt, track.RemovedBy }, e => e.Id == entity.Id);
        }
        else
        {
            // Db.Delete(entity);
        }
        CacheOnDelete(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Removed Entity by Object [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");
    }

    virtual public async Task RemoveAsync(Entity entity)
    {
        OnBeforeRemoving(entity);
        if (entity is Trackable track)
        {
            track.RemovedAt = DateTimeOffset.Now;
            track.RemovedBy = Auth?.UserName;
            // await Db.UpdateAsync<Entity>(new { IsDeleted = true, track.RemovedAt, track.RemovedBy }, e => e.Id == entity.Id);
        }
        else
        {
            // await Db.DeleteAsync(entity);
        }
        CacheOnDelete(entity);

        if (Log.IsDebugEnabled)
            Log.Info($"Removed Entity Async by Object [{entity.Id}] of Type: [{entity.EntityName}] by User: [{Auth?.UserName}]");
    }

    virtual public void RemoveAll()
    {
        //TODO: check if Entity is Trackable.
        // Db.DeleteAll<Entity>();

        CacheOnDeleteAll();

        if (Log.IsDebugEnabled)
            Log.Info($"Removed All of Type: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}]");
    }

    virtual public void CurrentDataToDBSeed()
    {
        var data = GetAll();
        var entityName = new Entity().EntityName;
        var path = "~/".MapHostAbsolutePath() + "/DBSeed/" + entityName + ".json";
        File.WriteAllText(path, data.ToJson().IndentJson());
    }

    #region Cache
    virtual public void CacheOnAdd(Entity entity)
    {
        if (CacheDisabled) return;

        if (Log.IsDebugEnabled)
            Log.Info($"Cache on Add of Type: [{entity}] by User: [{Auth?.UserName}]");

        // if (entity is BaseDocument document)
        //     document.RowVersion = (Db.SingleById<Entity>(entity.Id) as BaseDocument)!.RowVersion;

        var cacheGetAll = Cache!.Get<List<Entity>>(CACHE_GET_ALL());
        if (cacheGetAll != null)
        {
            cacheGetAll.Add(entity);
            Cache.Set(CACHE_GET_ALL(), cacheGetAll);
        }

        var cacheGetById = Cache.Get<Entity>(CACHE_GET_BY_ID() + entity.Id);
        if (cacheGetById != null)
            Cache.Set(CACHE_GET_BY_ID() + entity.Id, entity);

        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_GET_PAGED()));
        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_CUSTOM()));
    }

    virtual public void CacheOnUpdate(Entity entity)
    {
        if (CacheDisabled) return;

        if (Log.IsDebugEnabled)
            Log.Info($"Cache on Update of Type: [{entity}] by User: [{Auth?.UserName}]");

        // if (entity is BaseDocument document)
        //     document.RowVersion = (Db.SingleById<Entity>(entity.Id) as BaseDocument)!.RowVersion;

        var cacheGetAll = Cache!.Get<List<Entity>>(CACHE_GET_ALL());
        if (cacheGetAll != null)
        {
            if (cacheGetAll.Exists(e => e.Id == entity.Id))
            {
                var toUpdate = cacheGetAll.FindIndex(e => e.Id == entity.Id);
                cacheGetAll[toUpdate] = entity;
            }
            else
            {
                cacheGetAll.Add(entity);
            }
            Cache.Set(CACHE_GET_ALL(), cacheGetAll);
        }

        var cacheGetById = Cache.Get<Entity>(CACHE_GET_BY_ID() + entity.Id);
        if (cacheGetById != null)
            Cache.Set(CACHE_GET_BY_ID() + entity.Id, entity);

        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_GET_PAGED()));
        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_CUSTOM()));
    }

    virtual public void CacheOnDelete(Entity entity)
    {
        if (CacheDisabled) return;

        if (Log.IsDebugEnabled)
            Log.Info($"Cache on Delete of Type: [{entity}] by User: [{Auth?.UserName}]");

        var cacheGetAll = Cache!.Get<List<Entity>>(CACHE_GET_ALL());
        if (cacheGetAll != null)
        {
            var toRemove = cacheGetAll.Find(e => e.Id == entity.Id);
            if (toRemove != null) cacheGetAll.Remove(toRemove);
            Cache.Set(CACHE_GET_ALL(), cacheGetAll);
        }

        Cache.Remove(CACHE_GET_BY_ID() + entity.Id);

        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_GET_PAGED()));
        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_CUSTOM()));
    }

    virtual public void CacheOnDeleteAll()
    {
        if (CacheDisabled) return;

        if (Log.IsDebugEnabled)
            Log.Info($"Cache on Delete All: [{EntityInfo.EntityName}] by User: [{Auth?.UserName}]");

        FlushServiceCache();
    }

    public static void FlushServiceCache(params long[] ids)
    {
        if (Log.IsDebugEnabled)
            Log.Info($"Flush Service Cache: [{typeof(Entity).Name}]");

        if (ids.Length > 0)
            foreach (var id in ids)
                Cache!.Remove(CACHE_GET_BY_ID() + id);
        else
            Cache!.RemoveAll(Cache.GetKeysStartingWith(CACHE_GET_BY_ID()));

        Cache!.Remove(CACHE_GET_ALL());
        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_GET_PAGED()));
        Cache.RemoveAll(Cache.GetKeysStartingWith(CACHE_CUSTOM()));
    }
    #endregion
}
