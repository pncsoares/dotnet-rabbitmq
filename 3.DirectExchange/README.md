# Direct exchange

Direct exchange uses routing keys in the header to identify which queue the message should be sent to.

Routing key is a header value set by the producer. The consumer uses the routing key to bind to the queue. The exchange does exact match of routing key values.

# Overview of the code

## Producer

We will start to create a static class named `DirectExchangePublisher.cs` and move the code from the class `QueueProducer.cs` into this new class.

Then we need to change some things in order to change from a queue to an exchange. To start, we need to declare the exchange:

```csharp
channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct);
```

We set the name of the exchange and the type that we will use to this example, the direct exchange.

Now we can delete the method `QueueDeclare` because we no longer are using a queue. 

Next step is to change the method `BasicPublish`. This time, when we publish the messages, we want to use the previously declared exchange. And we also need to use a routing key, let's name ours as `"account.init"`:

```csharp
channel.BasicPublish("demo-direct-exchange", "account.init", null, body);
```

The last step is to use the `DirectExchangePublisher` instead of the `QueueProducer` in the `Program.cs`:

```csharp
DirectExchangePublisher.Publish(channel);
```

And that's it for the producer. The final code is:

`DirectExchangePublisher.cs`

```csharp
public static class DirectExchangeProducer
{
    public static void Publish(IModel channel)
    {
        channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct);

        var count = 0;

        while (true)
        {
            var message = new
            {
                Name = "Producer",
                Message = $"#{count} Hello World!"
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish("demo-direct-exchange", "account.init", null, body);

            count++;

            Thread.Sleep(1000);
        }
    }
}
```

`Program.cs`

```csharp
var factory = new ConnectionFactory
{
    // We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

DirectExchangePublisher.Publish(channel);
```

## Consumer

We will start to create a static class named `DirectExchangeConsumer.cs` and move the code from the class `QueueConsumer.cs` into this new class.

Then we need to change some things in order to change from a queue to an exchange. As we did in the producer, we also need to declare the exchange in the consumer:

```csharp
channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct);
```

We set the name of the exchange and the type that we will use to this example, the direct exchange.

But now, differently from the producer, we will need the `QueueDeclare` so we will keep it. 

Next step is to bind all the components: queue, exchange and routing key:

```csharp
channel.QueueBind("demo-direct-queue", "demo-direct-exchange", "account.init");
```

Now, we need to change the method `BasicConsume`. We just need to use the new queue name:

```csharp
channel.BasicConsume("demo-direct-queue", true, consumer);
```

The last step is to use the `DirectExchangeConsumer` instead of the `QueueConsumer` in the `Program.cs`:

```csharp
DirectExchangePublisher.Publish(channel);
```

And that's it for the producer. The final code is:

`DirectExchangePublisher.cs`

```csharp
public static class DirectExchangeConsumer
{
    public static void Consume(IModel channel)
    {
        channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct);
        channel.QueueDeclare("demo-direct-queue", true, false, false, null);
        channel.QueueBind("demo-direct-queue", "demo-direct-exchange", "account.init");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine(message);
        };

        channel.BasicConsume("demo-direct-queue", true, consumer);

        Console.WriteLine("Consumer started");

        Console.ReadKey();
    }
}
```

`Program.cs`

```csharp
var factory = new ConnectionFactory
{
    // We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

DirectExchangeConsumer.Publish(channel);
```

Done ✅

# RabbitMQ console

Lets run the consumer and analyze the RabbitMQ console 👇

Exchange:

![Direct Exchange](../.github/images/direct-exchange.png)

If we look inside the exchange, we can see it's properties that were configured by us in the code:

![Direct Exchange Properties](../.github/images/direct-exchange-properties.png)

And the queue:

![Direct Queue](../.github/images/direct-queue.png)

> ### 📝 Note:
>
> So, now, if we open multiple consumers, all of them will receive the messages evenly but only because we have one type of consumer. If you create more consumer with other configurations/properties, you will see that the messages will only be read by the consumer that is binded to the queue that the producer sends to it.

This is how the direct exchange works 😊

## Lifetime of a message

Now, lets configure the lifetime of the messages so we can see it in action. We need to declare a argument attribute in the exchange but before we declare the TTL.

This argument attribute is set on the producer.

> If you don't know what the TTL is, read the following [section](https://github.com/pncsoares/dotnet-rabbitmq#lifetime-of-a-message).

```csharp
var ttl = new Dictionary<string, object>
{
    { "x-message-ttl", 30000 }
};

channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct, arguments: ttl);
```

## Prefetch count

To configure the prefetch count, we need to use a method named `BasicQos` in the channel property.

`BasicQos` method parameters:
- prefetchSize = This field specifies the prefetch window size in octets. The server will send a message in advance if it is equal to or smaller in size than the available prefetch size (and also falls into other prefetch limits). May be set to zero, meaning "no specific limit", although other prefetch limits may still apply.
- prefetchCount = the number of messages that we want to fetch
- global
  - `true` = shared across all consumers on the channel
  - `false` = shared across all consumers on the connection

This method must be added on the consumer.

> If you don't know what the prefetch count is, read the following [section](https://github.com/pncsoares/dotnet-rabbitmq#prefetch-count).

```csharp
channel.BasicQos(0, 10, false);
```

This will make this consumer to fetch 10 messages at a time.

> For this to take effect we need to delete the queue and the exchange:
>
> ![Delete Direct Queue](../.github/images/delete-direct-queue.gif)
>
> ![Delete Direct Exchange](../.github/images/delete-direct-exchange.gif)
>
> And now you must start the producer first. The result must be the following:
>
> ### TTL
>
> ![Direct Exchange TTL](../.github/images/direct-exchange-ttl.png)
>
> ![Direct Exchange TTL Details](../.github/images/direct-exchange-ttl-details.png)
>
> ### Prefetch count
>
> ![Direct Exchange Prefetch Count](../.github/images/direct-exchange-prefetch-count.png)

# Test it yourself

You can now run the Consumer console application and then run the Producer console application and see the message displayed in the Consumer's console.

To do that, you can use your IDE buttons/options or via terminal, executing the following command:

> before executing the command, `cd` to this repository root directory

```bash
# consumer
dotnet run --project .\3.DirectExchange\Producer\Producer.csproj

# producer
dotnet run --project .\3.DirectExchange\Consumer\Consumer.csproj
```

> Open multiple tabs of the consumer and run the project so you can see the messages being delivered to all of the consumers

The behavior here is exactly the same as the [2.SingleProducerMultipleConsumers](https://github.com/pncsoares/dotnet-rabbitmq/tree/main/2.SingleProducerMultipleConsumers#interesting-behavior-) 👇

## Consumer 1

```
Consumer started
{"Name":"Producer","Message":"#0 Hello World!"}
{"Name":"Producer","Message":"#2 Hello World!"}
{"Name":"Producer","Message":"#4 Hello World!"}
{"Name":"Producer","Message":"#6 Hello World!"}
{"Name":"Producer","Message":"#8 Hello World!"}
{"Name":"Producer","Message":"#10 Hello World!"}
```

## Consumer 2

```
Consumer started
{"Name":"Producer","Message":"#1 Hello World!"}
{"Name":"Producer","Message":"#3 Hello World!"}
{"Name":"Producer","Message":"#5 Hello World!"}
{"Name":"Producer","Message":"#7 Hello World!"}
{"Name":"Producer","Message":"#9 Hello World!"}
{"Name":"Producer","Message":"#11 Hello World!"}
```

Check out other examples that we have created.