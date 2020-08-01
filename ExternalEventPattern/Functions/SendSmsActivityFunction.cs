using System;
using System.Threading.Tasks;
using ExternalEventPattern.Configs;
using ExternalEventPattern.Models;
using ExternalEventPattern.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ExternalEventPattern.Functions
{
    public class SendSmsActivityFunction
    {
        private readonly SmsConfiguration _smsConfiguration;
        private readonly ISendSmsService _sendSmsService;
        private readonly ILogger<SendSmsActivityFunction> _logger;

        public SendSmsActivityFunction(ISendSmsService sendSmsService, ILogger<SendSmsActivityFunction> logger)
        {   
            _sendSmsService = sendSmsService;
            _logger = logger;
        }

        [FunctionName(nameof(SendSmsActivityFunction))]
        public async Task SendSmsAsync([ActivityTrigger] IDurableActivityContext context,
            [DurableClient]IDurableOrchestrationClient client
            /*[TwilioSms(From = "%SmsConfiguration.From%")]IAsyncCollector<CreateMessageOptions> messages*/)
        {
            var recipient = context.GetInput<SmsRecipient>();

            //var message = new CreateMessageOptions(new PhoneNumber(recipient.To))
            //{
            //    Body = recipient.Message
            //};

            //await messages.AddAsync(message);

            var status = await _sendSmsService.SendAsync(new SendSmsRequest
            {
                From = recipient.From,
                To = recipient.To,
                Message = recipient.Message
            });

            await client.RaiseEventAsync(context.InstanceId, "smssentevent", new SmsSentEvent
            {
                Id = recipient.Id,
                Status = status
            });
        }
    }
}