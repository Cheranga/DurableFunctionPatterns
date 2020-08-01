using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalEventPattern.DataAccess;
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

            var status = await context.CallActivityAsync<bool>(nameof(SaveSmsRecipientActivityFunction), recipient);
            if (!status)
            {
                return;
            }

            using (var cts = new CancellationTokenSource())
            {
                var timeoutTask = context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(30), cts.Token);
                var waitForSmsSendTask = context.WaitForExternalEvent<SmsEvent>("smssentevent");

                var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 5);
                await context.CallActivityWithRetryAsync(nameof(SendSmsActivityFunction), retryOptions, recipient);
                
                var winner = await Task.WhenAny(timeoutTask, waitForSmsSendTask);
                if (winner == waitForSmsSendTask)
                {
                    cts.Cancel();

                    await context.CallActivityAsync(nameof(UpdateSmsRecipientActivityFunction), waitForSmsSendTask.Result);
                }
                else
                {   
                    await context.CallActivityAsync(nameof(UpdateSmsRecipientActivityFunction), new SmsEvent{Id = recipient.Id, Status = RecipientStatus.CouldNotSend});
                }
            }
        }
    }
}