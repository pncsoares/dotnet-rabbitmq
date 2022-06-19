using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer;

public static class DirectExchangeConsumer
{
    public static void Consume(IModel channel)
    {
        channel.ExchangeDeclare(Shared.Constants.DirectExchange.ExchangeName, ExchangeType.Direct);
        channel.QueueDeclare(Shared.Constants.DirectExchange.QueueName, true, false, false, null);
        channel.QueueBind(Shared.Constants.DirectExchange.QueueName, Shared.Constants.DirectExchange.ExchangeName, Shared.Constants.DirectExchange.RoutingKey);
        channel.BasicQos(0, 10, false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine(message);
        };

        channel.BasicConsume(Shared.Constants.DirectExchange.QueueName, true, consumer);

        Console.WriteLine("Consumer started");

        Console.ReadKey();
    }
}