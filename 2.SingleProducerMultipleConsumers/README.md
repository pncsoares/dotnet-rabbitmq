# Single producer with multiple consumers

To this example, we will reuse most of the code of the Single producer and consumer example, the simplest one but we will organize it a little bit to understand it better.

# Overview of the code

## Producer

We will start to create a new class named `QueueProducer.cs`. This class will be static and will have a method:

```csharp
public static void Publish()
{

}
```

Now we will move the following code, from `Program.cs` into this new method:

```csharp
channel.QueueDeclare("demo-queue", true, false, false, null);

var message = new
{
    Name = "Producer",
    Message = "Hello World!"
};

var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

channel.BasicPublish(string.Empty, "demo-queue", null, body);
```

And in order for it to work, we need to receive a parameter:

```csharp
public static void Publish(IModel channel)
{
    channel.QueueDeclare("demo-queue", true, false, false, null);

    var message = new
    {
        Name = "Producer",
        Message = "Hello World!"
    };

    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

    channel.BasicPublish(string.Empty, "demo-queue", null, body);
}
```

Once this is done, we need to invoke this static method in the `Program.cs` class:

```csharp
var factory = new ConnectionFactory
{
    // We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

QueueProducer.Publish(channel); // new line
```

Then, in the `QueueProducer`, `Publish` method what we want to do is to publish multiple messages at the same time, and to do that, we will change the code:

```csharp
public static void Publish(IModel channel)
{
    channel.QueueDeclare("demo-queue", true, false, false, null);

    var count = 0;

    while (true)
    {
        var message = new
        {
            Name = "Producer",
            Message = $"#{count} Hello World!"
        };

        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        channel.BasicPublish(string.Empty, "demo-queue", null, body);

        count++;
        
        Thread.Sleep(1000);
    }
}
```

Now we have a while loop and, every second, because of the `Thread.Sleep(1000);` a new message will be published into the queue. Every message will have a count so we can distinguish them but we are using an integer just for the sake of simplicity.

## Consumer

In the consumer we will start to create a new class named `QueueConsumer.cs`. This class will be static and will have a method:

```csharp
public static void Consume()
{

}
```

Now we will move the following code, from `Program.cs` into this new method:

```csharp
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
```

And in order for it to work, we need to receive a parameter:

```csharp
public static void Consume(IModel channel)
{
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
}
```

Now we just need to invoke this new method in the `Program.cs` file:

```csharp
var factory = new ConnectionFactory
{
    // We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

QueueConsumer.Consume(channel); // new line
```

# Test it yourself

You can now run the Consumer console application and then run the Producer console application and see the message displayed in the Consumer's console.

To do that, you can use your IDE buttons/options or via terminal, executing the following command:

> before executing the command, `cd` to this repository root directory

```bash
# consumer
dotnet run --project .\2.SingleProducerMultipleConsumers\Producer\Producer.csproj

# producer
dotnet run --project .\2.SingleProducerMultipleConsumers\Consumer\Consumer.csproj
```

> Open multiple tabs of the consumer and run the project so you can see the messages being delivered to all of the consumers

Example of messages being delivered into multiple consumers:

## Consumer 1

```
Consumer started
{"Name":"Producer","Message":"#0 Hello World!"}
{"Name":"Producer","Message":"#2 Hello World!"}
{"Name":"Producer","Message":"#4 Hello World!"}
{"Name":"Producer","Message":"#6 Hello World!"}
{"Name":"Producer","Message":"#8 Hello World!"}
{"Name":"Producer","Message":"#10 Hello World!"}
{"Name":"Producer","Message":"#12 Hello World!"}
{"Name":"Producer","Message":"#14 Hello World!"}
{"Name":"Producer","Message":"#16 Hello World!"}
{"Name":"Producer","Message":"#18 Hello World!"}
{"Name":"Producer","Message":"#20 Hello World!"}
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
{"Name":"Producer","Message":"#13 Hello World!"}
{"Name":"Producer","Message":"#15 Hello World!"}
{"Name":"Producer","Message":"#17 Hello World!"}
{"Name":"Producer","Message":"#19 Hello World!"}
{"Name":"Producer","Message":"#21 Hello World!"}
```

## Interesting behavior 🤔

Notice that the two consumers are getting the messages in a round-robin fashion. The consumer 1 only received even messages and the consumer 2 only received odd messages.

So, if we have multiple consumers reading from a single queue, the messages will be evenly distributed across the consumers.

> ### ⚠️ Important note
> 
> This behavior is very important because when it comes to scaling a service it is extremely important that we have the capability to scale it horizontally and RabbitMQ gives us exactly that functionality and as you can see if we have too many messages in the queue, we can add more consumers to the same queue and start processing the messages in parallel and it will also ensure that one consumer gets a unique message and the same message is not delivered to multiple consumers.

Take a look at RabbitMQ interface (http://localhost:15672) to see the messages being produced to the queue and then consumed by multiple consumers:

![Message produced and consumed by multiple consumers](../.github/images/produced-message-consumed-message-multiple-consumers.png)

Check out other examples that we have created.