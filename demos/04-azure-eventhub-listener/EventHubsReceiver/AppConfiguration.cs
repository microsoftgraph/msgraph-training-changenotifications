using System;
using System.Collections.Generic;
using System.Text;

namespace EventHubsReceiver
{
    class AppConfiguration
    {
        public string EventHubConnectionString { get; set; }
        public string EventHubName { get; set; }
        public string BlobStorageConnectionString { get; set; }
        public string BlobContainerName { get; set; }
    }
}
