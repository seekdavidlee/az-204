// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var connection = config["Settings:ConnectionString"];
var queueName = config["Settings:QueueName"];
var serviceBus = new ServiceBusClient(connection);
var sender = serviceBus.CreateSender(queueName);

while (true)
{
    Console.WriteLine("Enter iterations:");
    var lines = Console.ReadLine();

    Console.WriteLine("Enter prefix:");
    var prefix = Console.ReadLine();

    var tasks = new List<Task>();
    for (int i = 0; i < Convert.ToInt32(lines); i++)
    {
        tasks.Add(sender.SendMessageAsync(new ServiceBusMessage($"{prefix}{i}")));        
    }
    await Task.WhenAll(tasks);
}
