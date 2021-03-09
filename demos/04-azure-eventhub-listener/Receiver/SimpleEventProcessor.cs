using System;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Receiver
{
    class SimpleEventProcessor : IEventProcessor
    {
        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            return Task.CompletedTask;
        }


        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine("SimpleEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {

            if (messages != null)
            {
                foreach (EventData eventData in messages)
                {
                    string eventdata = Encoding.UTF8.GetString(eventData.GetBytes());
                    Console.WriteLine("eventData = {0}", eventdata);
                }
            }
            return context.CheckpointAsync();
        }

        /*private static void getAndDownloadFromBlob(PartitionContext context, string key, string storageConnectionString, string containerName)
        {
            CloudStorageAccount sa = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient bc = sa.CreateCloudBlobClient();
            CloudBlobContainer container = bc.GetContainerReference(containerName);
            Task<bool> doesContainerExist = container.GetBlockBlobReference(key).ExistsAsync();
            doesContainerExist.Wait();
            if (doesContainerExist.Result)
            {
                CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(key);
                Task<string> payloadDownloaded = downloadFromBlobAsync(cloudBlockBlob);
                payloadDownloaded.Wait();
                string payloadDownloadedFromBlob = payloadDownloaded.Result;
                Console.WriteLine("Payload Downloaded : {0} , partition_id = {1} ", payloadDownloadedFromBlob, context.Lease.PartitionId);
            }
        }

        static async Task<string> downloadFromBlobAsync(CloudBlockBlob cloudBlockBlob)
        {
            //Console.WriteLine("{0} > Downloading from Blob");

            string payloadToDownload = await cloudBlockBlob.DownloadTextAsync();
            return payloadToDownload;
        } */
    }
}
