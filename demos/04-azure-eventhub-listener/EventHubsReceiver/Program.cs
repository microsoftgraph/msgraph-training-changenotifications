using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http.Headers;
using System.Text;
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

            //Console.WriteLine("Create a subscription and then press any key to continue..");
            //Console.ReadKey();
            // Create a Subscription
            string subId = await CreateSubscription(graphServiceClient);
            Console.WriteLine($"Subscription created with {subId}");

            // Prepare to Receive messages (notifications)
            Console.WriteLine($"Starting the event hub receiver");
            SimpleEventProcessor simpleEventProcessor = new SimpleEventProcessor(_appConfiguration);
            await simpleEventProcessor.ReceiveMessages();
            Console.WriteLine($"Event hub receiver started...");

            // Start user creation and deletion activities to trigger messages
            UserHelper userHelper = new UserHelper(graphServiceClient, _appConfiguration.TenantDomain);
            await userHelper.GraphDeltaQueryExample();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task SendMessages()
        {
            // Create a producer client that you can use to send events to an event hub
            await using (var producerClient = new EventHubProducerClient(_appConfiguration.EventHubConnectionString, _appConfiguration.EventHubName))
            {
                // Create a batch of events
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                // Add events to the batch. An event is a represented by a collection of bytes and metadata.
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("First event")));
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Second event")));
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Third event")));

                // Use the producer client to send the batch of events to the event hub
                await producerClient.SendAsync(eventBatch);
                Console.WriteLine("A batch of 3 events has been published.");
            }
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