// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;
using System.Threading;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace msgraphapp.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class NotificationsController : ControllerBase
  {
    private readonly MyConfig config;
    private static Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
    private static Timer? subscriptionTimer = null;

    public NotificationsController(MyConfig config)
    {
      this.config = config;
    }

    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
      var graphServiceClient = GetGraphClient();

      var sub = new Microsoft.Graph.Subscription();
      sub.ChangeType = "updated";
      sub.NotificationUrl = config.Ngrok + "/api/notifications";
      sub.Resource = "/users";
      sub.ExpirationDateTime = DateTime.UtcNow.AddMinutes(15);
      sub.ClientState = "SecretClientState";

      var newSubscription = await graphServiceClient
        .Subscriptions
        .Request()
        .AddAsync(sub);

      Subscriptions[newSubscription.Id] = newSubscription;

      if (subscriptionTimer == null)
      {
        subscriptionTimer = new Timer(CheckSubscriptions, null, 5000, 15000);
      }

      return $"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}";
    }

    public async Task<ActionResult<string>> Post([FromQuery] string? validationToken = null)
    {
      // handle validation
      if (!string.IsNullOrEmpty(validationToken))
      {
        Console.WriteLine($"Received Token: '{validationToken}'");
        return Ok(validationToken);
      }

      // handle notifications
      using (StreamReader reader = new StreamReader(Request.Body))
      {
        string content = await reader.ReadToEndAsync();

        Console.WriteLine(content);

        var notifications = JsonSerializer.Deserialize<ChangeNotificationCollection>(content);

        if (notifications != null)
        {
          foreach (var notification in notifications.Value)
          {
            Console.WriteLine($"Received notification: '{notification.Resource}', {notification.ResourceData.AdditionalData["id"]}");
          }
        }
      }

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

    private void CheckSubscriptions(Object? stateInfo)
    {
      AutoResetEvent? autoEvent = stateInfo as AutoResetEvent;

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

    private async void RenewSubscription(Subscription subscription)
    {
      Console.WriteLine($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

      var graphServiceClient = GetGraphClient();

      var newSubscription = new Subscription
      {
        ExpirationDateTime = DateTime.UtcNow.AddMinutes(5)
      };

      await graphServiceClient
        .Subscriptions[subscription.Id]
        .Request()
        .UpdateAsync(newSubscription);

      subscription.ExpirationDateTime = newSubscription.ExpirationDateTime;
      Console.WriteLine($"Renewed subscription: {subscription.Id}, New Expiration: {subscription.ExpirationDateTime}");
    }

  }
}