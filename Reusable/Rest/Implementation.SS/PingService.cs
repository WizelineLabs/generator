namespace Reusable.Rest.Implementations.SS;

public class PingService : Service
{
    public object Any(Ping request)
    {
        return new { Result = "System up and running!" };
    }
}

[Route("/Ping")]
public class Ping { }
