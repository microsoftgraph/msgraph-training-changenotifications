using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubsReceiver
{
    /// <summary>
    /// SimpleEventProcessor is an implementation that hooks with an Event Hub and processes data in it.
    /// https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/eventhub/Azure.Messaging.EventHubs/MigrationGuide.md
    /// </summary>
    public class SimpleEventProcessor
    {

        ConcurrentDictionary<string, int> partitionEventCount = new ConcurrentDictionary<string, int>();
        BlobContainerClient storageClient = null;
        AppConfiguration _appConfiguration;
        EventProcessorClient processor = null;

        public SimpleEventProcessor(AppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;

            storageClient = new BlobContainerClient(_appConfiguration.StorageConnectionString, _appConfiguration.BlobContainerName);

            // Read from the default consumer group: $Default
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            processor = new EventProcessorClient(storageClient, consumerGroup, _appConfiguration.EventHubConnectionString, _appConfiguration.EventHubName);
        }

        public async Task processEventHandler(ProcessEventArgs args)
        {
            try
            {
                // If the cancellation token is signaled, then the
                // processor has been asked to stop.  It will invoke
                // this handler with any events that were in flight;
                // these will not be lost if not processed.
                //
                // It is up to the handler to decide whether to take
                // action to process the event or to cancel immediately.

                if (args.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                string partition = args.Partition.PartitionId;
                byte[] eventBody = args.Data.EventBody.ToArray();
                Console.WriteLine($"An event from partition { partition } with length { eventBody.Length }.");

                string eventdata = Encoding.UTF8.GetString(args.Data.EventBody.ToArray());
                Console.WriteLine("eventData = {0}", eventdata);

                int eventsSinceLastCheckpoint = partitionEventCount.AddOrUpdate(
                    key: partition,
                    addValue: 1,
                    updateValueFactory: (_, currentCount) => currentCount + 1);

                if (eventsSinceLastCheckpoint >= 2)
                {
                    await args.UpdateCheckpointAsync();
                    partitionEventCount[partition] = 0;
                }
            }
            catch
            {
                // It is very important that you always guard against
                // exceptions in your handler code; the processor does
                // not have enough understanding of your code to
                // determine the correct action to take.  Any
                // exceptions from your handlers go uncaught by
                // the processor and will NOT be redirected to
                // the error handler.
            }
        }

        Task processErrorHandler(ProcessErrorEventArgs args)
        {
            try
            {
                Console.WriteLine("Error in the EventProcessorClient");
                Console.WriteLine($"\tOperation: { args.Operation }");
                Console.WriteLine($"\tException: { args.Exception }");
                Console.WriteLine("");
            }
            catch
            {
                // It is very important that you always guard against
                // exceptions in your handler code; the processor does
                // not have enough understanding of your code to
                // determine the correct action to take.  Any
                // exceptions from your handlers go uncaught by
                // the processor and will NOT be handled in any
                // way.
            }

            return Task.CompletedTask;
        }

        public async Task ProcessMessages()
        {
            try
            {
                using var cancellationSource = new CancellationTokenSource();
                cancellationSource.CancelAfter(TimeSpan.FromSeconds(30));

                processor.ProcessEventAsync += processEventHandler;
                processor.ProcessErrorAsync += processErrorHandler;

                try
                {
                    await processor.StartProcessingAsync(cancellationSource.Token);
                    await Task.Delay(Timeout.Infinite, cancellationSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // This is expected if the cancellation token is
                    // signaled.
                }
                finally
                {
                    // This may take up to the length of time defined
                    // as part of the configured TryTimeout of the processor;
                    // by default, this is 60 seconds.

                    await processor.StopProcessingAsync();
                }
            }
            catch
            {
                // The processor will automatically attempt to recover from any
                // failures, either transient or fatal, and continue processing.
                // Errors in the processor's operation will be surfaced through
                // its error handler.
                //
                // If this block is invoked, then something external to the
                // processor was the source of the exception.
            }
            finally
            {
                // It is encouraged that you unregister your handlers when you have
                // finished using the Event Processor to ensure proper cleanup.  This
                // is especially important when using lambda expressions or handlers
                // in any form that may contain closure scopes or hold other references.

                processor.ProcessEventAsync -= processEventHandler;
                processor.ProcessErrorAsync -= processErrorHandler;
            }
        }

    }
}