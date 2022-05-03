namespace Reusable.CRUD.Implementations.EF;

using Reusable.EmailServices;
using Reusable.Utils;
using Reusable.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public abstract class BaseLogic : Contract.ILogic
{
    public static IAppSettings? AppSettings { get; set; }
    public IEmailService? EmailService { get; set; }
    public bool CacheDisabled { get; set; }
    public static TimeSpan CacheExpiresIn = new TimeSpan(12, 0, 0);
    public static ICacheClient? Cache { get; set; }
    public IAuthSession? Auth { get; set; }
    public ILog Log;
    public  Log<BaseLogic> Logger;
    public DbContext DbContext { get; set; }

    protected BaseLogic(DbContext DbContext, ILog logger)
    {
        this.DbContext = DbContext;
        // Log = new Log<BaseLogic>(logger);
        Log = logger;
    }

    // public IRequest Request { get; set; }
    // public IDbConnection Db { get; set; }
    // public Service? Service { get; set; }

    public virtual string ToMD5(object from) => MD5HashGenerator.GenerateKey(from);

    public virtual void Init(IDbConnection db, Service service)
    {
        CacheDisabled = AppSettings!.Get<bool>("cacheDisabled") == true;
        // Db = db;
        // Auth = service.GetSession();
        // Request = service.Request;
        // Service = service;
    }

    public bool HasRoles(params string[] roles)
    {
        if (Auth != null && Auth.Roles != null)
        {
            foreach (var role in roles)
                if (!Auth.Roles.Any(r => r.ToLower() == role.ToLower()))
                    return false;
        }
        else
            return false;

        return true;
    }

    static public object? TryCatch(Action operation)
    {
        try
        {
            operation();
        }
        catch (Exception ex)
        {
            // Log.Error(ex.Message, ex);
            return ex.Message;
        }
        return null;
    }

    // [RuntimeSerializable]
    public class FilterResponse
    {
        public int total_items { get; set; }
        public int total_filtered_items { get; set; }
        public int page { get; set; }
    }

    public Exception GetOriginalException(Exception ex)
    {
        if (ex.InnerException == null) return ex;

        return GetOriginalException(ex.InnerException);
    }

    protected bool IsValidJSValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "null" || value == "undefined")
        {
            return false;
        }

        return true;
    }

    protected bool IsValidParam(string param)
    {
        //reserved and invalid params:
        if (new string?[] {
                "limit",
                "perPage",
                "page",
                "search",
                "itemsCount",
                "noCache",
                "totalItems",
                "parentKey",
                "parentField",
                "filterUser",
                "RequiresKeysInJsons",
                "OrderBy",
                "OrderByDesc",
                null
            }.Contains(param))
            return false;

        return true;
    }

    protected bool IsValidCacheParam(string param)
    {
        //reserved and invalid params:
        if (new string?[] {
                "itemsCount",
                "noCache",
                "totalItems",
                null
            }.Contains(param))
            return false;

        return true;
    }
}
