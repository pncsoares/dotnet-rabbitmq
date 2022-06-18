using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    Uri = new Uri(Shared.Constants.Uri)
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();
channel.QueueDeclare(Shared.Constants.QueueName, true, false, false, null);

var message = new
{
    Name = "Producer",
    Message = "Hello World!"
};

var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

channel.BasicPublish(string.Empty, Shared.Constants.QueueName, null, body);