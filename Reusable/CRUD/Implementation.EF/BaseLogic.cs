namespace Reusable.CRUD.Implementations.EF;

using Reusable.EmailServices;
using Reusable.Utils;
using Reusable.Contract;
using Microsoft.EntityFrameworkCore;

public abstract class BaseLogic : Contract.ILogic
{
    public static IAppSettings? AppSettings { get; set; }
    protected readonly IConfiguration Configuration;
    public IEmailService? EmailService { get; set; }
    public bool CacheDisabled { get; set; }
    public static TimeSpan CacheExpiresIn = new TimeSpan(12, 0, 0);
    public static ICacheClient? Cache { get; set; }
    public IAuthSession? Auth { get; set; }
    public ILog Log = null!;
    public DbContext DbContext { get; set; }

    protected BaseLogic(DbContext DbContext, ILog logger, IConfiguration configuration)
    {
        this.DbContext = DbContext;
        Log = logger;
        Configuration = configuration;
        CacheDisabled = configuration.GetValue<bool>("CACHE_DISABLED", true) == true;
    }

    public virtual string ToMD5(object from) => MD5HashGenerator.GenerateKey(from);

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
