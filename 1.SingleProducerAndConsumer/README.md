# Single producer and consumer

To build this, we have two .NET Core Console Applications.

# Overview of the code

In order for us to work with RabbitMQ we need to install the RabbitMQ.Client nuget package: https://www.nuget.org/packages/rabbitmq.client in both producer and consumer projects.

> To keep things simple, we will do all the code in the `Program.cs` file of each console application but when you need to build a mechanism like this, consider creating classes and abstractions/interfaces.

## Producer

We will start to create a connection factory:

```csharp
var factory = new ConnectionFactory
{
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};
```

> ☝️ We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}

Now, we only need to create the connection:

```csharp
using var connection = factory.CreateConnection();
```

> We use using because we want do dispose the connection

Now, we need to create a channel:

```csharp
using var channel = connection.CreateModel();
```

If you read the documentation of the `CreateModel` method you will see: _"Create and return a fresh channel, session, and model."_

Now that we have access to the model, we will create a queue:

```csharp
channel.QueueDeclare("demo-queue", true, false, false, null);
```

`QueueDeclare` method parameters:
- queue = name of the queue, `demo-queue` for our example
- durable = `true` because we want the message to hang around there until a consumer reads it
- exclusive = `false`
- autoDelete = `false`
- arguments = `null`

> The last 3 parameters, `exclusive`, `autoDelete` and `arguments` will be important when we see the Direct Exchange example

Now, to this basic example, we have all that we need and it's time to publish a message to this queue so lets start to create a message:

```csharp
var message = new
{
    Name = "Producer",
    Message = "Hello World!"
};
```

And now we will convert it to a byte array:

```csharp
var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
```

> To do this, we will also need to install the Newtonsoft.Json nuget package: https://www.nuget.org/packages/Newtonsoft.Json

Now lets publish the message to the queue:

```csharp
channel.BasicPublish(string.Empty, "demo-queue", null, body);
```

`BasicPublish` method parameters:
- exchange = name of the exchange - we will not use exchange in this example but we will for example in the Direct Exchange example
- routingKey = name of the queue
- basicProperties = `null`
- body = the message in `ReadOnlyMemory<byte>`, that's why we converted the message into a byte array

Now we just need a consumer to read the message from the queue.

## Consumer

We will start to copy and paste all the code from the producer's `Program.cs` and paste it into the consumer's `Program.cs` - this is repeated code but it is only for the sake of the example.

Now that we pasted the code, we just need to delete the following code from our consumer's `Program.cs`:

```csharp
var message = new
{
    Name = "Producer",
    Message = "Hello World!"
};

var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

channel.BasicPublish(string.Empty, "demo-queue", null, body);
```

And we end up with the following code:

```csharp
var factory = new ConnectionFactory
{
    // We are using AMQP pattern: amqp://{username}:{guest}@{hostname}:{port}
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();
channel.QueueDeclare("demo-queue", true, false, false, null);
```

We will now create a consumer:

```csharp
var consumer = new EventingBasicConsumer(channel);
```

> The method `EventingBasicConsumer` takes a IModel so we will pass the output of the `connection.CreateModel();`

Now, we need to create an event for when the consumer reads and receives a message from the queue. We will use the method `Received` to add a new event, and if you read the documentation it says _"Event fired when a delivery arrives for the consumer. Handlers must copy or fully use delivery body before returning."_.

```csharp
consumer.Received += (sender, e) =>
{
    var body = e.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine(message);
};
```

> For the sake of the example we will just print the message into the console without deserializing it but in a real application we may consider deserializing it and probably save it into a database or so.

Now that we have a consumer and an event that will be triggered when a message arrives, we just need to invoke a method to actually consume the message:

```csharp
channel.BasicConsume("demo-queue", true, consumer);
```

`BasicConsume` method parameters:
- queue = name of the queue
- autoAck = `true` (read more about this property in the official documentation [here](https://www.rabbitmq.com/confirms.html#consumer-acknowledgements))
- consumer = the consumer

To finish we will only add a `Console.ReadKey();` so the consumer terminal not close automatically.

# Test it yourself

You can now run the Consumer console application and then run the Producer console application and see the message displayed in the Consumer's console.

To do that, you can use your IDE buttons/options or via terminal, executing the following command:

> before executing the command, `cd` to this repository root directory

```bash
# consumer
dotnet run --project .\1.SingleProducerAndConsumer\Producer\Producer.csproj

# producer
dotnet run --project .\1.SingleProducerAndConsumer\Consumer\Consumer.csproj
```

Example of a message:

```
{"Name":"Producer","Message":"Hello World!"}
```

Take a look at RabbitMQ interface (http://localhost:15672) to see the message being produced to the queue and then consumed:

![Message produced and consumed](../.github/images/produced-message-consumed-message.png)

And that's it. This is the simplest and easiest example 🐥

Take a look at the other examples that we have created.