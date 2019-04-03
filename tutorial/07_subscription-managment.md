<!-- markdownlint-disable MD002 MD041 -->

### Manage notification subscriptions

Subscriptions for notifications expire and need to be renewed periodically.

Open **NotificationsController.cs** and replace the `Get()` method with the following code:

```csharp
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
```

Add the following using statement at the top of the file.

```csharp
using System.Threading;
```

This code above adds a background timer that will fire every 15 seconds and check subscriptions to see if they have expired.

Add the following new methods:

```csharp
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
```

The `CheckSubscriptions` is called every 15 seconds by the timer. The `RenewSubscription` method renews a subscription and is only called if a subscription is going to expire in the next two minutes.

Select **Debug > Start debugging** to run the application. Navigate to the following url `http://localhost:5000/api/notifications` to register a new subscription.

You will see the following output in the `DEBUG OUTPUT` window of Visual Studio Code approximately every 15 seconds.  This is the timer checking the subscription for expiry.

```shell
Checking subscriptions 12:32:51.882
Current subscription count 1
```

Wait a few minutes and you will see the following when the subscription needs renewing:

```shell
Renewed subscription: 07ca62cd-1a1b-453c-be7b-4d196b3c6b5b, New Expiration: 3/10/2019 7:43:22 PM +00:00
```

This indicates that the subscription was renewed and shows the new expiry time.
