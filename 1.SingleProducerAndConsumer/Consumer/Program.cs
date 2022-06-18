using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory
{
    Uri = new Uri(Shared.Constants.Uri)
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();
channel.QueueDeclare(Shared.Constants.QueueName, true, false, false, null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
{
    var body = e.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine(message);
};

channel.BasicConsume(Shared.Constants.QueueName, true, consumer);

Console.ReadKey();