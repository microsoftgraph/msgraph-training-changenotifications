using System;
using System.Collections.Generic;
using System.Text;

namespace EventHubsReceiver
{
    public class AppConfiguration
    {
        public string EventHubConnectionString { get; set; }
        public string EventHubName { get; set; }
        public string StorageConnectionString { get; set; }
        
        /// <summary>
        /// The name of the Blob container attached with the EventHub
        /// </summary>
        public string BlobContainerName { get; set; }

        /// <summary>
        /// Client Id of the app used to create Graph subscriptions in the tenant
        /// </summary>
       public string ClientId { get; set; }

        /// <summary>
        /// The Tenant/Directory domain, like contoso.onmicrosoft.com
        /// </summary>
        public string TenantDomain { get; set; }

        /// <summary>
        /// The redirect used in the Azure Ad app's registration
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// The Graph subscription's notification URL
        /// </summary>
        public string NotificationUrl { get; set; }
    }
}
