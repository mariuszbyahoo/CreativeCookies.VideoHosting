using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure
{
    public interface IMessageQueueReceiver
    {
        Task StartAsync();
        Task StopAsync();
        Task MessageHandler(ProcessMessageEventArgs args);
        Task ErrorHandler(ProcessErrorEventArgs args);
    }
}
