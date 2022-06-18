using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer;

public static class DirectExchangeConsumer
{
    public static void Consume(IModel channel)
    {
        channel.ExchangeDeclare(Shared.Constants.DirectExchangeName, ExchangeType.Direct);
        channel.QueueDeclare(Shared.Constants.DirectQueueName, true, false, false, null);
        channel.QueueBind(Shared.Constants.DirectQueueName, Shared.Constants.DirectExchangeName, Shared.Constants.DirectRoutingKey);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine(message);
        };

        channel.BasicConsume(Shared.Constants.DirectQueueName, true, consumer);

        Console.WriteLine("Consumer started");

        Console.ReadKey();
    }
}