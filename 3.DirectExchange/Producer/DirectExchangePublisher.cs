using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Producer;

public static class DirectExchangePublisher
{
    public static void Publish(IModel channel)
    {
        var ttl = new Dictionary<string, object>
        {
            { "x-message-ttl", 30000 }
        };
        
        channel.ExchangeDeclare(Shared.Constants.DirectExchange.ExchangeName, ExchangeType.Direct, arguments: ttl);

        var count = 0;

        while (true)
        {
            var message = new
            {
                Name = "Producer",
                Message = $"#{count} Hello World!"
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish(Shared.Constants.DirectExchange.ExchangeName, Shared.Constants.DirectExchange.RoutingKey, null, body);

            count++;

            Thread.Sleep(1000);
        }
    }
}