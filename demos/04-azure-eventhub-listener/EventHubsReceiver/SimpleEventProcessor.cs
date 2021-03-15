using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventHubsReceiver
{
    /// <summary>
    /// SimpleEventProcessor is an implementation that hooks with an Event Hub and processes data in it.
    /// </summary>
    public class SimpleEventProcessor
    {
        private AppConfiguration _appConfiguration;

        public SimpleEventProcessor(AppConfiguration appConfiguration)
        {
            this._appConfiguration = appConfiguration;
        }

        /// <summary>
        /// code frome https://docs.microsoft.com/en-ca/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send
        /// </summary>
        /// <returns></returns>
        public async Task ReceiveMessages()
        {
            // Read from the default consumer group: $Default
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            // Create a blob container client that the event processor will use
            BlobContainerClient storageClient = new BlobContainerClient(_appConfiguration.BlobStorageConnectionString, _appConfiguration.BlobContainerName);

            // Create an event processor client to process events in the event hub
            EventProcessorClient processor = new EventProcessorClient(storageClient, consumerGroup, _appConfiguration.EventHubConnectionString, _appConfiguration.EventHubName);

            // Register handlers for processing events and handling errors
            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            // Start the processing
            await processor.StartProcessingAsync();

            // Wait for 10 seconds for the events to be processed
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Stop the processing
            await processor.StopProcessingAsync();
        }

        private static async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));

            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        private static Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }
}