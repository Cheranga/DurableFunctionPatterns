using System;
using System.Threading.Tasks;
using ExternalEventPattern.Models;
using Microsoft.Extensions.Logging;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ExternalEventPattern.Services
{
    public class SendSmsService : ISendSmsService
    {
        private readonly ITwilioRestClient _twilioRestClient;
        private readonly ILogger<SendSmsService> _logger;

        public SendSmsService(ITwilioRestClient twilioRestClient, ILogger<SendSmsService> logger)
        {
            _twilioRestClient = twilioRestClient;
            _logger = logger;
        }

        public async Task<bool> SendAsync(SendSmsRequest request)
        {
            try
            {
                var operation = await MessageResource.CreateAsync(new PhoneNumber(request.To),
                    from: new PhoneNumber(request.From),
                    body: request.Message,
                    client: _twilioRestClient);

                var status = operation.Status == MessageResource.StatusEnum.Accepted;

                return status;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when sending sms");
            }

            return false;
        }
    }
}