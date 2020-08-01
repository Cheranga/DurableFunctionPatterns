using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalEventPattern.Models;
using ExternalEventPattern.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace ExternalEventPattern.Functions
{
    public class SendSmsOrchestratorFunction
    {   

        [FunctionName(nameof(SendSmsOrchestratorFunction))]
        public async Task SendSmsAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var recipient = context.GetInput<SmsRecipient>();

            var status = await context.CallActivityAsync<bool>(nameof(CreateRecipientFunction), recipient);
            if (!status)
            {
                return;
            }

            using (var cts = new CancellationTokenSource())
            {
                var timeoutTask = context.CreateTimer(context.CurrentUtcDateTime.AddMinutes(1), cts.Token);
                var waitForSmsSendTask = context.WaitForExternalEvent<SmsSentEvent>("smssentevent");

                await context.CallActivityAsync(nameof(SendSmsActivityFunction), recipient);

                var winner = await Task.WhenAny(timeoutTask, waitForSmsSendTask);
                if (winner == waitForSmsSendTask)
                {
                    cts.Cancel();
                    //
                    // Update the storage depending on the message sent status.
                    //
                }
                else
                {
                    //
                    // Update the storage stating that the message could not be sent in the said interval
                    //
                }
            }
        }
    }

    public class CreateRecipientFunction
    {
        [FunctionName(nameof(CreateRecipientFunction))]
        public async Task<bool> CreateRecipientAsync([ActivityTrigger] IDurableActivityContext context)
        {
            var recipient = context.GetInput<SmsRecipient>();
            //
            // TODO:
            // Create the customer in the storage.
            //
            await Task.Delay(TimeSpan.FromSeconds(2));

            return true;
        }
    }
}