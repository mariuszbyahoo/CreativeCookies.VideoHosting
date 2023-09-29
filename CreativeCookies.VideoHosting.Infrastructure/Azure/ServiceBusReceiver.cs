using Azure.Messaging.ServiceBus;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Azure
{
    public class ServiceBusReceiver : IMessageQueueReceiver
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _processor;
        private readonly ILogger _logger;

        public event Action<string> OnMessageReceived;

        public ServiceBusReceiver(IConfiguration configuration, ILogger<ServiceBusReceiver> logger)
        {
            _logger = logger;
            _serviceBusClient = new ServiceBusClient(configuration.GetValue<string>("ServiceBusConnectionString"));
            _processor = _serviceBusClient.CreateProcessor("stripe_events_queue", new ServiceBusProcessorOptions());

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
        }
        public async Task StartAsync()
        {
            await _processor.StartProcessingAsync();
        }

        public async Task StopAsync()
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
            OnMessageReceived?.Invoke(messageBody);
            await args.CompleteMessageAsync(args.Message);
        }
    }
}
