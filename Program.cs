using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;

Console.WriteLine("=== Hello, this is a simple service bus client to check connectivity from AWS to Azure to consume message");

ServiceBusClient client;
ServiceBusSender sender;
ServiceBusProcessor processor;

static string GetEnvironmentVariable(string name, string defaultValue)
    => Environment.GetEnvironmentVariable(name) ?? defaultValue;

int numberOfMessages = (int)Int32.Parse(GetEnvironmentVariable("NUM_MSG", "3"));

var clientOptions = new ServiceBusClientOptions()
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};


var connectionString = Environment.GetEnvironmentVariable("ASB_CONN");
client = new ServiceBusClient(connectionString, clientOptions);
var queueName = "sbq-emaildelivery-dev";
sender = client.CreateSender(queueName);
processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
for (int i = 1; i <= numberOfMessages; i++)
{
    var timestamp = DateTime.Now;
    if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Test Message {i} {timestamp}")))
    {
        throw new Exception($"=== The message {i} is too large to fit in the batch.");
    }
}

try
{
    await sender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"=== A batch of {numberOfMessages} messages has been published to queue");

    Console.WriteLine("=== Now it's time to receive messages");
    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler;
    await processor.StartProcessingAsync();
    await Task.Delay(TimeSpan.FromSeconds(3));
    await processor.StopProcessingAsync();
}
finally
{
    await sender.DisposeAsync();
    await processor.DisposeAsync();
    await client.DisposeAsync();
}


async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"=== Received: {body}");
    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine($"=== Error: {args.Exception.ToString()}");
    return Task.CompletedTask;
}