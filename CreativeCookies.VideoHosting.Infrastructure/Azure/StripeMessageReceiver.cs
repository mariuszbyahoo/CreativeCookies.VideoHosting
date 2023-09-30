using Azure.Messaging.ServiceBus;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Azure
{
    public class StripeMessageReceiver : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _processor;
        private readonly ILogger _logger;


        public StripeMessageReceiver(IConfiguration configuration, ILogger<StripeMessageReceiver> logger)
        {
            _logger = logger;
            _serviceBusClient = new ServiceBusClient(configuration.GetValue<string>("ServiceBusConnectionString"));
            _processor = _serviceBusClient.CreateProcessor("stripe_events_queue", new ServiceBusProcessorOptions());
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
        }

        public async Task StartAsync(CancellationToken token)
        {
            await _processor.StartProcessingAsync();
        }

        public async Task StopAsync(CancellationToken token)
        {
            await _processor.StopProcessingAsync();
            await _serviceBusClient.DisposeAsync();
        }

        public async Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError($"An unexpected exception occured inside of a ServiceBusReceiver, {args.Exception.Message}, {args.Exception.Source}, {args.Exception.StackTrace}, {args.Exception.InnerException}, {args.Exception.Data}");
        }

        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string messageBody = args.Message.Body.ToString();
            // there has to be some service which would then process the coming Stripe Events
            // HACK: Do something with the message.
            await args.CompleteMessageAsync(args.Message);
        }
    }
}
