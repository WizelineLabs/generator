using Reusable.Contract;
using Microsoft.Extensions.Logging;
namespace Reusable.Utils;
public class Log<T> : ILog
{
    public Log(ILogger<T> logger)
    {
        Logger = logger;
        
    }
    public bool IsDebugEnabled => true;

    public ILogger<T> Logger { get; }

    public void Debug(object message)
    {
        Logger.LogDebug(message.ToString());
    }
    public void Debug(object message, Exception exception)
    {
        Logger.LogDebug(exception, message.ToString());
    }
    public void Error(object message)
    {
        Logger.LogError(message.ToString());
    }
    public void Error(object message, Exception exception)
    {
        Logger.LogError(exception, message.ToString());
    }
    public void Fatal(object message)
    {
         Logger.LogCritical(message.ToString());
    }
    public void Fatal(object message, Exception exception)
    {
         Logger.LogCritical(exception, message.ToString());
    }
    public void Info(object message)
    {
         Logger.LogInformation(message.ToString());
    }
    public void Info(object message, Exception exception)
    {
         Logger.LogInformation(exception, message.ToString());
    }
    public void Warn(object message)
    {
         Logger.LogWarning(message.ToString());
    }
    public void Warn(object message, Exception exception)
    {
         Logger.LogWarning(exception, message.ToString());
    }
}