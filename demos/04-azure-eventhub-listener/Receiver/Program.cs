using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;
using System.Threading.Tasks;



namespace Receiver
{




    class Program
    {
        static async Task Main(string[] args)
        {
            string eventHubConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];
            string eventHubName = ConfigurationManager.AppSettings["EventHubName"];
            string storageAccountName = ConfigurationManager.AppSettings["StorageAccountName"];
            string storageAccountKey = ConfigurationManager.AppSettings["StorageAccountKey"];

            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);

            Console.WriteLine("Registering EventProcessor...");
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();
            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();


            await eventProcessorHost.UnregisterEventProcessorAsync();
        }

    }
}
