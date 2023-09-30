using Azure.Messaging.ServiceBus;
using CreativeCookies.StripeEvents.Contracts;
using CreativeCookies.StripeEvents.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.StripeEvents.Services.HostedServices
{
    public class StripeMessageReceiver : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _processor;
        private readonly IStripeEventsDistributor _eventsDistributor;
        private readonly ILogger _logger;


        public StripeMessageReceiver(IStripeEventsDistributor eventsDistributor, IConfiguration configuration, ILogger<StripeMessageReceiver> logger)
        {
            _logger = logger;
            _eventsDistributor = eventsDistributor;
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
            var stripeEventDto = System.Text.Json.JsonSerializer.Deserialize<StripeEventDTO>(args.Message.Body.ToString());
            await _eventsDistributor.RedirectEvent(stripeEventDto);
            await args.CompleteMessageAsync(args.Message);
        }
    }
}
