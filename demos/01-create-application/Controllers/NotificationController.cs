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
          ExpirationDateTime = DateTime.UtcNow + new TimeSpan(1, 0, 0, 0),
          ClientState = "SecretClientState"
        };

        // POST to the graph to create the subscription
        var response = client.PostAsJsonAsync<Subscription>("/v1.0/subscriptions", subscription).Result;
        var responseContent = response.Content.ReadAsStringAsync().Result;

        // deserialize the response
        var subscriptionDetail = JsonConvert.DeserializeObject<Subscription>(responseContent);

        return $"Subscribed. Id: {subscriptionDetail.Id}";
      }
    }

    //POST ?validationToken={opaqueTokenCreatedByMicrosoftGraph}    [HttpPost]
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
      return Ok();
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