using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer;

public static class QueueConsumer
{
    public static void Consume(IModel channel)
    {
        channel.QueueDeclare(Shared.Constants.QueueName, true, false, false, null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine(message);
        };

        channel.BasicConsume(Shared.Constants.QueueName, true, consumer);

        Console.WriteLine("Consumer started");

        Console.ReadKey();
    }
}