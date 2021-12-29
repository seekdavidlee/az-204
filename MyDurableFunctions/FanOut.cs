using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MyDurableFunctions
{
    public class FanOutData
    {
        public int Value { get; set; }
    }

    public static class FanOut
    {
        [FunctionName("FanOut")]
        public static async Task<int> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var parallelTasks = new List<Task<int>>
            {
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 12 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 30 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 100 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 11 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 3 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 10 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 172 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 301 }),
                context.CallActivityAsync<int>("FanOutActivity", new FanOutData { Value = 101 })
            };

            await Task.WhenAll(parallelTasks);

            return parallelTasks.Sum(x => x.Result);
        }

        [FunctionName("FanOutActivity")]
        public static int NextNumber([ActivityTrigger] IDurableActivityContext obj)
        {
            FanOutData data = obj.GetInput<FanOutData>();
            return data.Value + 1;
        }

        [FunctionName("FanOutRequest")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FanOut", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}