namespace Shared;

public static class Constants
{
    /// <summary>
    /// Using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    /// </summary>
    public const string Uri = "amqp://guest:guest@localhost:5672";
    
    public const string QueueName = "demo-queue";

    public static class DirectExchange
    {
        public const string ExchangeName = "demo-direct-exchange";

        public const string QueueName = "demo-direct-queue";

        public const string RoutingKey = "account.init";    
    }
}