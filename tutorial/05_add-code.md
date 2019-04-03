<!-- markdownlint-disable MD002 MD041 -->

Open the **Startup.cs** file and comment out the following line to disable ssl redirection.

```csharp
//app.UseHttpsRedirection();
```

### Add model classes

The application uses several new model classes for (de)serialization of messages to/from the Microsoft Graph.

Right-click in the project file tree and select **New Folder**. Name it **Models**
Right-click the **Models** folder and add four new files:

- **Notification.cs**
- **ResourceData.cs**
- **Subscription.cs**
- **GraphToken.cs**

Replace the contents of **Notification.cs** with the following:

```csharp
using Newtonsoft.Json;
using System;

namespace msgraphapp.Models
{
  public class Notifications
  {
    [JsonProperty(PropertyName = "value")]
    public Notification[] Items { get; set; }
  }

  // A change notification.
  public class Notification
  {
    // The type of change.
    [JsonProperty(PropertyName = "changeType")]
    public string ChangeType { get; set; }

    // The client state used to verify that the notification is from Microsoft Graph. Compare the value received with the notification to the value you sent with the subscription request.
    [JsonProperty(PropertyName = "clientState")]
    public string ClientState { get; set; }

    // The endpoint of the resource that changed. For example, a message uses the format ../Users/{user-id}/Messages/{message-id}
    [JsonProperty(PropertyName = "resource")]
    public string Resource { get; set; }

    // The UTC date and time when the webhooks subscription expires.
    [JsonProperty(PropertyName = "subscriptionExpirationDateTime")]
    public DateTimeOffset SubscriptionExpirationDateTime { get; set; }

    // The unique identifier for the webhooks subscription.
    [JsonProperty(PropertyName = "subscriptionId")]
    public string SubscriptionId { get; set; }

    // Properties of the changed resource.
    [JsonProperty(PropertyName = "resourceData")]
    public ResourceData ResourceData { get; set; }
  }
}
```

Replace the contents of **ResourceData.cs** with the following:

```csharp
using Newtonsoft.Json;

namespace msgraphapp.Models
{
  public class ResourceData
  {
    // The ID of the resource.
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    // The OData etag property.
    [JsonProperty(PropertyName = "@odata.etag")]
    public string ODataEtag { get; set; }

    // The OData ID of the resource. This is the same value as the resource property.
    [JsonProperty(PropertyName = "@odata.id")]
    public string ODataId { get; set; }

    // The OData type of the resource: "#Microsoft.Graph.Message", "#Microsoft.Graph.Event", or "#Microsoft.Graph.Contact".
    [JsonProperty(PropertyName = "@odata.type")]
    public string ODataType { get; set; }
  }
}
```

Replace the contents of **Subscription.cs** with the following:

```csharp
using Newtonsoft.Json;
using System;

namespace msgraphapp.Models
{
  public class Subscription
  {
    // The type of change in the subscribed resource that raises a notification.
    [JsonProperty(PropertyName = "changeType")]
    public string ChangeType { get; set; }

    // The string that Microsoft Graph should send with each notification. Maximum length is 255 characters.
    // To verify that the notification is from Microsoft Graph, compare the value received with the notification to the value you sent with the subscription request.
    [JsonProperty(PropertyName = "clientState")]
    public string ClientState { get; set; }

    // The URL of the endpoint that receives the subscription response and notifications. Requires https.
    // This can include custom query parameters.
    [JsonProperty(PropertyName = "notificationUrl")]
    public string NotificationUrl { get; set; }

    // The resource to monitor for changes.
    [JsonProperty(PropertyName = "resource")]
    public string Resource { get; set; }

    // The amount of time in UTC format when the webhook subscription expires, based on the subscription creation time.
    // The maximum time varies for the resource subscribed to. This sample sets it to the 4230 minute maximum for messages.
    // See https://developer.microsoft.com/graph/docs/api-reference/v1.0/resources/subscription for maximum values for resources.
    [JsonProperty(PropertyName = "expirationDateTime")]
    public DateTimeOffset ExpirationDateTime { get; set; }

    // // The unique identifier for the webhook subscription.
    // [JsonProperty(PropertyName = "id")]
    [JsonProperty("id", NullValueHandling=NullValueHandling.Ignore)]
    public string Id { get; set; }
  }
}
```

Finally, replace the contents of **GraphToken.cs** with the following:

```csharp
using Newtonsoft.Json;
using System;

namespace msgraphapp.Models
{
  // A change notification.
  public class GraphToken
  {
    // The type of change.
    [JsonProperty(PropertyName = "access_token")]
    public string AccessToken { get; set; }
  }
}
```

### Add notification controller

The application requires a new controller to process the subscription and notification.

Right-click the `Controllers` folder, select **New File**, and name the controller **NotificationsController.cs**.

Replace the contents of **NotificationController.cs** with the following:

```csharp
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
```

In **NotificationsController.cs** replace the following variables with the values you copied earlier:

- `<NGROK URL>` should be set to the https ngrok url you copied earlier.
- `<TENANT ID>` should be your Office 365 tenant id, for example. **contoso.onmicrosoft.com**.
- `<APP ID>` and `<APP SECRET>` should be the application id and secret you copied earlier when you created the application registration.

```csharp
private const string ApiUrl = "<NGROK URL>";
private const string TenantId = "<TENANT ID>";
private const string AppId = "<APP ID>";
private const string AppSecret = "<APP SECRET>";
```

**Save** all files.