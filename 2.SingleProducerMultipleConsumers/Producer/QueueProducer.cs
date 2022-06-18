using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Producer;

public static class QueueProducer
{
    public static void Publish(IModel channel)
    {
        channel.QueueDeclare(Shared.Constants.QueueName, true, false, false, null);

        var count = 0;

        while (true)
        {
            var message = new
            {
                Name = "Producer",
                Message = $"#{count} Hello World!"
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish(string.Empty, Shared.Constants.QueueName, null, body);

            count++;
            
            Thread.Sleep(1000);
        }
    }
}