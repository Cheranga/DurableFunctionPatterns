using System.Net.Http;
using System.Threading.Tasks;
using ExternalEventPattern.Configs;
using Twilio.Clients;
using Twilio.Http;
using HttpClient = Twilio.Http.HttpClient;

namespace ExternalEventPattern.Services
{
    public class CustomTwilioClient : ITwilioRestClient
    {
        private readonly ITwilioRestClient _innerClient;

        public CustomTwilioClient(SmsConfiguration smsConfiguration, System.Net.Http.HttpClient httpClient, IHttpClientFactory httpClientFactory)
        {
            var twilioClient = httpClientFactory.CreateClient("twilioclient");
            _innerClient = new TwilioRestClient(smsConfiguration.AccountSid, smsConfiguration.AuthToken, httpClient:new SystemNetHttpClient(twilioClient));
        }

        public Response Request(Request request)
        {
            return _innerClient.Request(request);
        }

        public Task<Response> RequestAsync(Request request)
        {
            return _innerClient.RequestAsync(request);
        }

        public string AccountSid => _innerClient.AccountSid;
        public string Region => _innerClient.Region;
        public HttpClient HttpClient => _innerClient.HttpClient;
    }
}