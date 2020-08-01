using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ExternalEventPattern.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExternalEventPattern.Functions
{
    public class SendSmsClientFunction
    {
        private readonly ILogger<SendSmsClientFunction> _logger;

        public SendSmsClientFunction(ILogger<SendSmsClientFunction> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(SendSmsClientFunction))]
        public async Task<IActionResult> SendSmsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customer/sms")]
            HttpRequest request,
            [DurableClient]IDurableOrchestrationClient client)
        {
            var content = await new StreamReader(request.Body).ReadToEndAsync();
            var smsRecipient = JsonConvert.DeserializeObject<SmsRecipient>(content);

            var instanceId = Guid.NewGuid().ToString("N");

            await client.StartNewAsync(nameof(SendSmsOrchestratorFunction), instanceId, smsRecipient);

            return new AcceptedResult();
        }
    }
}
