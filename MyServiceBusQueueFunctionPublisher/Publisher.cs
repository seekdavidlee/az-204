using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace MyServiceBusQueueFunctionPublisher
{
    public static class Publisher
    {
        [FunctionName("MyServiceBusMessagePublisher")]
        [return: ServiceBus("%QueueName%", ServiceBusEntityType.Queue, Connection = "ServiceBusConnection")]
        public static async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            return await new StreamReader(req.Body).ReadToEndAsync();
        }
    }
}
