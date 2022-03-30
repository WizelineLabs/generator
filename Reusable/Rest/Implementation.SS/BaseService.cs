namespace Reusable.Rest.Implementations.SS;

using Reusable.CRUD.Implementations.SS;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.Messaging;

public class BaseService<TLogic> : Service where TLogic : BaseLogic
{
    public IDbConnectionFactory? DbConnectionFactory { get; set; }
    public TLogic? Logic { get; set; }
    public IAutoQueryDb? AutoQuery { get; set; }
    public IMessageService? MQServer { get; set; }

    public IAppSettings? AppSettings { get; set; }
    public static ILog Log = LogManager.GetLogger("MyApp");

    protected async Task<object> WithDbAsync(Func<IDbConnection, Task<object>> operation)
    {
        using (var db = await DbConnectionFactory.OpenAsync())
        {
            Logic!.Init(db, this);
            return await operation(db);
        }
    }

    protected object WithDb(Func<IDbConnection, object> operation)
    {
        using (var db = DbConnectionFactory.Open())
        {
            Logic!.Init(db, this);
            return operation(db);
        }
    }

    protected object InTransaction(Func<IDbConnection, object> operation, Func<object, object>? afterTransaction = null)
    {
        LocalCache.FlushAll();
        return WithDb(db =>
        {
            using (var transaction = db.OpenTransaction())
            {
                try
                {
                    var result = operation(db);
                    transaction.Commit();
                    if (afterTransaction != null)
                        return afterTransaction(result);
                    else
                        return result;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Error(ex, ex.Message);
                    throw;
                }
            }
        });
    }

    protected void WithDb(Action<IDbConnection> operation)
    {
        using (var db = DbConnectionFactory.Open())
        {
            Logic!.Init(db, this);
            operation(db);
        }
    }

    protected void InTransaction(Action<IDbConnection> operation, Action? afterTransaction = null)
    {
        LocalCache.FlushAll();
        WithDb(db =>
        {
            using (var transaction = db.OpenTransaction())
            {
                try
                {
                    operation(db);
                    transaction.Commit();
                    if (afterTransaction != null)
                        afterTransaction();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Error(ex, ex.Message);
                    throw;
                }
            }
        });
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
