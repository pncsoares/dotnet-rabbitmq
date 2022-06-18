using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    // We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();
channel.QueueDeclare("demo-queue", true, false, false, null);

var message = new
{
    Name = "Producer",
    Message = "Hello World!"
};

var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

channel.BasicPublish(string.Empty, "demo-queue", null, body);