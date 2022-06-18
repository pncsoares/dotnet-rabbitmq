using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory
{
    // We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();
channel.QueueDeclare("demo-queue", true, false, false, null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
{
    var body = e.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine(message);
};

channel.BasicConsume("demo-queue", true, consumer);

Console.ReadKey();