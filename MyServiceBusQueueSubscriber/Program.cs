// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var connection = config["Settings:ConnectionString"];
var queueName = config["Settings:QueueName"];
var serviceBus = new ServiceBusClient(connection);
var rec = serviceBus.CreateReceiver(queueName);

while (true)
{
    var message = await rec.ReceiveMessageAsync(TimeSpan.FromSeconds(30));

    if (message == null) continue;

    string line = Encoding.UTF8.GetString(message.Body.ToArray());
    Console.WriteLine($"{message.EnqueuedTime} - {message.EnqueuedSequenceNumber} - {line}");
    await rec.CompleteMessageAsync(message);
}
