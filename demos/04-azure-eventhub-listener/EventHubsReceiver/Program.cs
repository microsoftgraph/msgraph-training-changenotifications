using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubsReceiver
{
    internal class Program
    {
        private static IConfiguration configuration;
        private static AppConfiguration _appConfiguration;

        private static async Task Main()
        {
            // Using appsettings.json as our configuration settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");

            configuration = builder.Build();
            _appConfiguration = configuration.Get<AppConfiguration>();

            // Instantiate and prepare the Graph SDK
            var graphServiceClient = GetGraphServiceClient();

            // Create a Subscription
            string subId = await CreateSubscription(graphServiceClient);
            Console.WriteLine($"Subscription created with {subId}");

            // Prepare to Receive messages (notifications)
            Console.WriteLine($"Checking the event hub for messages");

            SimpleEventProcessor simpleEventProcessor = new SimpleEventProcessor(_appConfiguration);
            await simpleEventProcessor.ProcessMessages();
            //string eventProcessorHostName = Guid.NewGuid().ToString();
            //EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, _appConfiguration.EventHubName, EventHubConsumerGroup.DefaultGroupName, _appConfiguration.EventHubConnectionString, _appConfiguration.StorageConnectionString);
            //await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

                        //string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            //var consumer = new EventHubConsumerClient(consumerGroup, _appConfiguration.EventHubConnectionString, _appConfiguration.EventHubName);

            //try
            //{
            //    using CancellationTokenSource cancellationSource = new CancellationTokenSource();
            //    cancellationSource.CancelAfter(TimeSpan.FromSeconds(30));

            //    string firstPartition = (await consumer.GetPartitionIdsAsync(cancellationSource.Token)).First();
            //    EventPosition startingPosition = EventPosition.Earliest;

            //    await foreach (PartitionEvent partitionEvent in consumer.ReadEventsFromPartitionAsync(firstPartition, startingPosition, cancellationSource.Token))
            //    {
            //        string readFromPartition = partitionEvent.Partition.PartitionId;
            //        ReadOnlyMemory<byte> eventBodyBytes = partitionEvent.Data.EventBody.ToMemory();

            //        Console.WriteLine($"Read event of length { eventBodyBytes.Length } from { readFromPartition }");
            //        string eventdata = Encoding.UTF8.GetString(eventBodyBytes.ToArray());
            //        Console.WriteLine("eventData = {0}", eventdata);
            //    }
            //}
            //catch (TaskCanceledException)
            //{
            //    // This is expected if the cancellation token is
            //    // signaled.
            //}
            //finally
            //{
            //    await consumer.CloseAsync();
            //}

            Console.WriteLine($"Event hub receiver started...");

            // Start user creation and deletion activities to trigger messages
            UserHelper userHelper = new UserHelper(graphServiceClient, _appConfiguration.TenantDomain);
            await userHelper.GraphDeltaQueryExample();

            Console.WriteLine($"Checking the event hub for messages again");
            await simpleEventProcessor.ProcessMessages();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task<String> CreateSubscription(GraphServiceClient graphServiceClient)
        {
            var sub = new Microsoft.Graph.Subscription();
            sub.ChangeType = "Updated,Deleted";
            sub.NotificationUrl = _appConfiguration.NotificationUrl;
            sub.Resource = "/users";
            sub.ExpirationDateTime = DateTime.UtcNow.AddMinutes(15);
            sub.ClientState = "SecretClientState";

            var newSubscription = await graphServiceClient
              .Subscriptions
              .Request()
              .AddAsync(sub);

            return $"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}";
        }

        private static GraphServiceClient GetGraphServiceClient()
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                // get an access token for Graph
                await AuthenticateUsingMsalAsync();

                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", _GraphAccessToken);

                //return Task.FromResult(0);
            }));

            return graphClient;
        }

        public static string _GraphAccessToken = string.Empty;

        public static async Task<string> AuthenticateUsingMsalAsync()
        {
            if (!string.IsNullOrWhiteSpace(_GraphAccessToken))
            {
                return _GraphAccessToken;
            }

            string[] GraphScope = new string[] { $"https://graph.microsoft.com/.default" };
            IPublicClientApplication app = PublicClientApplicationBuilder.Create(_appConfiguration.ClientId)
             .WithAuthority(new Uri($"https://login.microsoftonline.com/{_appConfiguration.TenantDomain}"))
             .WithRedirectUri(_appConfiguration.RedirectUri)
             .Build();

            var GraphResult = await app.AcquireTokenInteractive(GraphScope).ExecuteAsync();

            if (GraphResult == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token for Graph");
            }

            _GraphAccessToken = GraphResult.AccessToken;

            return _GraphAccessToken;
        }
    }
}