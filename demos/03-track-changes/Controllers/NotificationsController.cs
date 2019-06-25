using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using msgraphapp.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Threading;

namespace msgraphapp.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class NotificationsController : ControllerBase
  {
    private readonly MyConfig config;

    public NotificationsController(MyConfig config)
    {
      this.config = config;
    }

    private static Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
    private static Timer subscriptionTimer = null;
    private static object DeltaLink = null;
    private static IUserDeltaCollectionPage lastPage = null;


    [HttpGet]
    public ActionResult<string> Get()
    {
      var graphServiceClient = GetGraphClient();

      var sub = new Microsoft.Graph.Subscription();
      sub.ChangeType = "updated";
      sub.NotificationUrl = config.Ngrok + "/api/notifications";
      sub.Resource = "/users";
      sub.ExpirationDateTime = DateTime.UtcNow.AddMinutes(5);
      sub.ClientState = "SecretClientState";

      var newSubscription = graphServiceClient
        .Subscriptions
        .Request()
        .AddAsync(sub).Result;

      Subscriptions[newSubscription.Id] = newSubscription;

      if (subscriptionTimer == null)
      {
        subscriptionTimer = new Timer(CheckSubscriptions, null, 5000, 15000);
      }

      return $"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}";
    }

    public ActionResult<string> Post([FromQuery]string validationToken = null)
    {
      // handle validation
      if(!string.IsNullOrEmpty(validationToken))
      {
        Console.WriteLine($"Received Token: '{validationToken}'");
        return Ok(validationToken);
      }

      // handle notifications
      using (StreamReader reader = new StreamReader(Request.Body))
      {
        string content = reader.ReadToEnd();

        Console.WriteLine(content);

        var notifications = JsonConvert.DeserializeObject<Notifications>(content);

        foreach(var notification in notifications.Items)
        {
          Console.WriteLine($"Received notification: '{notification.Resource}', {notification.ResourceData?.Id}");
        }
      }

      // use deltaquery to query for all updates
      CheckForUpdates();

      return Ok();
    }

    private GraphServiceClient GetGraphClient()
    {
      var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
      {
        // get an access token for Graph
        var accessToken = GetAccessToken().Result;

        requestMessage
            .Headers
            .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

        return Task.FromResult(0);
      }));

      return graphClient;
    }

    private async Task<string> GetAccessToken()
    {
      IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(config.AppId)
        .WithClientSecret(config.AppSecret)
        .WithAuthority($"https://login.microsoftonline.com/{config.TenantId}")
        .WithRedirectUri("https://daemon")
        .Build();

      string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

      var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

      return result.AccessToken;
    }

    private void CheckSubscriptions(Object stateInfo)
    {
      AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

      Console.WriteLine($"Checking subscriptions {DateTime.Now.ToString("h:mm:ss.fff")}");
      Console.WriteLine($"Current subscription count {Subscriptions.Count()}");

      foreach (var subscription in Subscriptions)
      {
        // if the subscription expires in the next 2 min, renew it
        if (subscription.Value.ExpirationDateTime < DateTime.UtcNow.AddMinutes(2))
        {
          RenewSubscription(subscription.Value);
        }
      }
    }

    private void RenewSubscription(Subscription subscription)
    {
      Console.WriteLine($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

      var graphServiceClient = GetGraphClient();

      subscription.ExpirationDateTime = DateTime.UtcNow.AddMinutes(5);

      var foo = graphServiceClient
        .Subscriptions[subscription.Id]
        .Request()
        .UpdateAsync(subscription).Result;

      Console.WriteLine($"Renewed subscription: {subscription.Id}, New Expiration: {subscription.ExpirationDateTime}");
    }

    private void CheckForUpdates()
    {
      var graphClient = GetGraphClient();

      // get a page of users
      var users = GetUsers(graphClient, DeltaLink);

      OutputUsers(users);

      // go through all of the pages so that we can get the delta link on the last page.
      while (users.NextPageRequest != null)
      {
        users = users.NextPageRequest.GetAsync().Result;
        OutputUsers(users);
      }

      object deltaLink;

      if (users.AdditionalData.TryGetValue("@odata.deltaLink", out deltaLink))
      {
        DeltaLink = deltaLink;
      }
    }

    private void OutputUsers(IUserDeltaCollectionPage users)
    {
      foreach(var user in users)
        {
          var message = $"User: {user.Id}, {user.GivenName} {user.Surname}";
          Console.WriteLine(message);
        }
    }

    private IUserDeltaCollectionPage GetUsers(GraphServiceClient graphClient, object deltaLink)
    {
      IUserDeltaCollectionPage page;

      if(lastPage == null)
      {
        page = graphClient
          .Users
          .Delta()
          .Request()
          .GetAsync()
          .Result;

      }
      else
      {
        lastPage.InitializeNextPageRequest(graphClient, deltaLink.ToString());
        page = lastPage.NextPageRequest.GetAsync().Result;
      }

      lastPage = page;
      return page;
    }
  }
}