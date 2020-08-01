using System;
using ExternalEventPattern;
using ExternalEventPattern.Configs;
using ExternalEventPattern.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddSingleton(new SmsConfiguration
            {
                AuthToken = Environment.GetEnvironmentVariable("SmsConfiguration.AuthToken"),
                AccountSid = Environment.GetEnvironmentVariable("SmsConfiguration.AccountSid"),
                FromNumber = Environment.GetEnvironmentVariable("SmsConfiguration.FromNumber")
            });
        }
    }
}