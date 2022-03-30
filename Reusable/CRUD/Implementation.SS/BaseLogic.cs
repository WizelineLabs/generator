namespace Reusable.CRUD.Implementations.SS;

using Reusable.EmailServices;
using Reusable.Rest;
using Reusable.Utils;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Logging;
using ServiceStack.Messaging;
using ServiceStack.Text;
using ServiceStack.Web;

public abstract class BaseLogic : Contract.ILogic
{
    public static IAppSettings? AppSettings { get; set; }
    public IRequest Request { get; set; }
    public IDbConnection Db { get; set; }
    public static ICacheClient? Cache { get; set; }
    public IAuthSession? Auth { get; set; }
    public IEmailService? EmailService { get; set; }
    public bool CacheDisabled { get; set; }
    public static ILog Log = LogManager.GetLogger("MyApp");
    public IMessageService? MQServer { get; set; }
    public static TimeSpan CacheExpiresIn = new TimeSpan(12, 0, 0);
    public Service? Service { get; set; }

    public virtual string ToMD5(object from) => MD5HashGenerator.GenerateKey(from);

    public virtual void Init(IDbConnection db, Service service)
    {
        Db = db;
        Auth = service.GetSession();
        Request = service.Request;
        CacheDisabled = AppSettings!.Get<bool>("cacheDisabled") == true;
        Service = service;
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
            Log.Error(ex.Message, ex);
            return ex.Message;
        }
        return null;
    }

    [RuntimeSerializable]
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

    public void WithMQ(Action<IMessageQueueClient> operation)
    {
        using (var mqClient = MQServer.CreateMessageQueueClient())
        {
            operation(mqClient);
        }
    }

    public void Publish(object toPublish)
    {
        using (var mqClient = MQServer.CreateMessageQueueClient())
        {
            mqClient.Publish(toPublish);
        }
    }

    public R? PublishSync<T, R>(object toPublish) where R : class
    {
        using (var mqClient = MQServer.CreateMessageQueueClient())
        {
            var replyTo = mqClient.GetTempQueueName();

            var dto = toPublish.ConvertTo<T>();

            // if (dto is IHasBearerToken hasBearer)
            //     hasBearer.BearerToken = Request.GetBearerToken();

            mqClient.Publish(new Message<T>(dto)
            {
                ReplyTo = replyTo
            });

            var msgResponse = mqClient.Get<R>(replyTo);

            var status = msgResponse?.Body?.GetResponseStatus();
            if (status?.ErrorCode != null)
                throw new KnownError(status.Message);

            return msgResponse?.Body as R;
        }
    }

    public object WithMQ(
        // string exchangeName, string queueName,
        Func<IMessageQueueClient, object> operation)
    {
        // var rabbitmqServer = MQServer as RabbitMqServer;
        // var conn = rabbitmqServer.ConnectionFactory.CreateConnection();

        // conn.OpenChannel();
        // using (var conn = rabbitmqServer.ConnectionFactory.CreateConnection())
        // using (var channel = conn.CreateModel())

        using (var mqClient = MQServer.CreateMessageQueueClient())
        {
            return operation(mqClient);
            // mqClient.Publish()
            // channel.ExchangeDeclare(exchange: exchangeName,
            //                         type: "direct",
            //                         durable: true,
            //                         autoDelete: false,
            //                         arguments: null);

            // channel.QueueDeclare(queue: queueName,
            //                     durable: true,
            //                     exclusive: false,
            //                     autoDelete: false,
            //                     arguments: null);

            // channel.QueueBind(queue: queueName,
            //                     exchange: exchangeName,
            //                     routingKey: queueName,
            //                     arguments: null);
        }
    }
}
