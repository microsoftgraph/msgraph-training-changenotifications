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

namespace msgraphapp.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class NotificationsController : ControllerBase
  {

    // for production use you should store these in application settings    
    private const string ApiUrl = "<NGROK URL>";
    private const string TenantId = "<TENANT ID>";
    private const string AppId = "<APP ID>";
    private const string AppSecret = "<APP SECRET>";

    private static Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
    private static Timer subscriptionTimer = null;

    [HttpGet]
    public ActionResult<string> Get()
    {
      // get an access token from graph
      var accessToken = GetAccessToken();

      // subscribe
      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri("https://graph.microsoft.com");
        client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");

        var subscription = new Subscription()
        {
          ChangeType = "updated",
          NotificationUrl = ApiUrl + "/api/notifications",
          Resource = "/users",
          ExpirationDateTime = DateTime.UtcNow.AddMinutes(5),
          ClientState = "SecretClientState"
        };

        // POST to the graph to create the subscription
        var response = client.PostAsJsonAsync<Subscription>("/v1.0/subscriptions", subscription).Result;
        var responseContent = response.Content.ReadAsStringAsync().Result;

        // deserialize the response
        var subscriptionDetail = JsonConvert.DeserializeObject<Subscription>(responseContent);

        Subscriptions[subscriptionDetail.Id] = subscriptionDetail;

        if(subscriptionTimer == null)
        {
            subscriptionTimer = new Timer(CheckSubscriptions, null, 5000, 15000);
        }

        return $"Subscribed. Id: {subscriptionDetail.Id}, Expiration: {subscriptionDetail.ExpirationDateTime}";
      }
    }

    private void CheckSubscriptions(Object stateInfo)
    {
      AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

      Console.WriteLine($"Checking subscriptions {DateTime.Now.ToString("h:mm:ss.fff")}");
      Console.WriteLine($"Current subscription count {Subscriptions.Count()}");

      foreach(var subscription in Subscriptions)
      {
        // if the subscription expires in the next 2 min, renew it
        if(subscription.Value.ExpirationDateTime < DateTime.UtcNow.AddMinutes(2))
        {
          RenewSubscription(subscription.Value);
        }
      }
    }

    private void RenewSubscription(Subscription subscription)
    {
      Console.WriteLine($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

      // get an access token from graph
      var accessToken = GetAccessToken();

      // renew subscription
      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri("https://graph.microsoft.com");
        client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");

        var subscriptionUpdate = new Subscription()
        {
          ExpirationDateTime = DateTime.UtcNow.AddMinutes(5),
        };

        var content = new ObjectContent<Subscription>(subscriptionUpdate, new JsonMediaTypeFormatter());

        // POST to the graph to create the subscription
        var response = client.PatchAsync($"/v1.0/subscriptions/{subscription.Id}", content).Result;
        var responseContent = response.Content.ReadAsStringAsync().Result;

        // deserialize the response
        var subscriptionDetail = JsonConvert.DeserializeObject<Subscription>(responseContent);

        // update the subscription
        subscription.ExpirationDateTime = subscriptionDetail.ExpirationDateTime;

        Console.WriteLine($"Renewed subscription: {subscription.Id}, New Expiration: {subscription.ExpirationDateTime}");
      }
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

    private static string DeltaLink = null;

    private void CheckForUpdates()
    {
      var uri = "";

      // get an access token from graph
      var accessToken = GetAccessToken();

      if(string.IsNullOrEmpty(DeltaLink))
      {
        uri = "https://graph.microsoft.com/v1.0/users/delta";
      }
      else
      {
        uri = DeltaLink;
      }

      Console.WriteLine($"Getting users: {uri}");

      while (true)
      {
        var users = GetChangedUsers(uri, accessToken);

        foreach(var user in users.Users)
        {
          Console.WriteLine($"User: {user.Id}, {user.GivenName} {user.Surname} Removed?:{user.Removed?.Reason}");
        }

        if(!string.IsNullOrEmpty(users.NextLink))
        {
          Console.WriteLine($"Got nextlink");
          uri = users.NextLink;
        }
        else
        {
          DeltaLink = users.DeltaLink;
          Console.WriteLine($"Got deltalink");
          break;
        }
      }
    }

    private UsersResponse GetChangedUsers(string uri, string accessToken)
    {
      using (var client = new HttpClient())
      {
        client.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");

        var result = client.GetAsync(uri).Result;
        string response = result.Content.ReadAsStringAsync().Result;

        var users = JsonConvert.DeserializeObject<UsersResponse>(response);

        return users;
      }
    }

    private string GetAccessToken()
    {
      var url = new Uri($"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token");
      string accessToken = "";

      var content = new FormUrlEncodedContent(new[]
      {
        new KeyValuePair<string, string>("client_id", AppId),
        new KeyValuePair<string, string>("client_secret", AppSecret),
        new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
        new KeyValuePair<string, string>("grant_type", "client_credentials")
      });
       
      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri("https://login.microsoftonline.com");

        var result = client.PostAsync($"/{TenantId}/oauth2/v2.0/token", content).Result;

        string tokenResult = result.Content.ReadAsStringAsync().Result;
        var token = JsonConvert.DeserializeObject<GraphToken>(tokenResult);

        accessToken = token.AccessToken;

        Console.WriteLine($"Got access token: {token.AccessToken}");
        return token.AccessToken;
      }
    }
  }
}