using System;
using System.Net.Http;
using ExternalEventPattern;
using ExternalEventPattern.Configs;
using ExternalEventPattern.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Twilio.Clients;

[assembly: FunctionsStartup(typeof(Startup))]

namespace ExternalEventPattern
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddScoped<ISendSmsService, SendSmsService>();
            services.AddHttpClient<ITwilioRestClient, CustomTwilioClient>();

            services
                .AddHttpClient("twilioclient", client => { client.DefaultRequestHeaders.Add("x-custom-header", "funkydory"); })
                .AddPolicyHandler(GetRetryPolicy());


            services.AddSingleton(new SmsConfiguration
            {
                AuthToken = Environment.GetEnvironmentVariable("AzureWebJobsTwilioAuthToken"),
                AccountSid = Environment.GetEnvironmentVariable("AzureWebJobsTwilioAccountSid"),
                FromNumber = Environment.GetEnvironmentVariable("SmsConfiguration.FromNumber")
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }
    }
}