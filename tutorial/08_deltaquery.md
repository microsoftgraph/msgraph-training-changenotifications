<!-- markdownlint-disable MD002 MD041 -->

### Query for changes

Microsoft Graph offers the ability to query for changes to a particular resource since you last called it. Using this option, combined with Change Notifications, enables a robust pattern for ensuring you don't miss any changes to the resources.

Locate and open the following controller: **Controllers > NotificationsController.cs**.
Add the following code to the existing `NotificationsController` class.

This code includes a new method, `CheckForUpdates()`, that will call the Microsoft Graph using the delta url and then pages through the results until it finds a new `deltalink` on the final page of results. It stores the url in memory until the code is notified again when another notification is triggered.

```csharp
private static object DeltaLink = null;

private static IUserDeltaCollectionPage lastPage = null;

private async Task CheckForUpdates()
{
    var graphClient = GetGraphClient();

    // get a page of users
    var users = await GetUsers(graphClient, DeltaLink);

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

private async Task<IUserDeltaCollectionPage> GetUsers(GraphServiceClient graphClient, object deltaLink)
{
    IUserDeltaCollectionPage page;

    if (lastPage == null)
    {
        page = await graphClient
            .Users
            .Delta()
            .Request()
            .GetAsync();

    }
    else
    {
        lastPage.InitializeNextPageRequest(graphClient, deltaLink.ToString());
        page = await lastPage.NextPageRequest.GetAsync();
    }

    lastPage = page;
    return page;
}
```

Locate the existing `Post()` method and replace it with the following code:

```csharp
public async Task<ActionResult<string>> Post([FromQuery]string validationToken = null)
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

        var notifications = JsonConvert.DeserializeObject<Notifications>(content);

        foreach (var notification in notifications.Items)
        {
            Console.WriteLine($"Received notification: '{notification.Resource}', {notification.ResourceData?.Id}");
        }
    }

    // use deltaquery to query for all updates
    await CheckForUpdates();            

    return Ok();
}
```

The `Post` method will now call `CheckForUpdates` when a notification is received. 

**Save** all files.

### Test your changes:

Within Visual Studio Code, select **Debug > Start debugging** to run the application.
Navigate to the following url: **http://localhost:5000/api/notifications**. This will register a new subscription.

Open a browser and navigate to the [Microsoft 365 admin center (https://admin.microsoft.com/AdminPortal)](https://admin.microsoft.com/AdminPortal).

1. If prompted to login, sign-in using an admin account.
1. Select **Users > Active users**. 
1. Select an active user and select **Edit** for their **Contact information**. 
1. Update the **Mobile phone** value with a new number and Select **Save**.

Wait for the notification to be received as indicated in the Visual Studio Code **Debug Console**:

```shell
Received notification: 'Users/7a7fded6-0269-42c2-a0be-512d58da4463', 7a7fded6-0269-42c2-a0be-512d58da4463
```

The application will now initiate a delta query with the graph to get all the users and log out some of their details to the console output.

```shell
User: 19e429d2-541a-4e0b-9873-6dff9f48fabe, Allan Deyoung
User: 05501e79-f527-4913-aabf-e535646d7ffa, Christie Cline
User: fecac4be-76e7-48ec-99df-df745854aa9c, Debra Berger
User: 4095c5c4-b960-43b9-ba53-ef806d169f3e, Diego Siciliani
User: b1246157-482f-420c-992c-fc26cbff74a5, Emily Braun
User: c2b510b7-1f76-4f75-a9c1-b3176b68d7ca, Enrico Cattaneo
User: 6ec9bd4b-fc6a-4653-a291-70d3809f2610, Grady Archie
User: b6924afe-cb7f-45a3-a904-c9d5d56e06ea, Henrietta Mueller
User: 0ee8d076-4f13-4e1a-a961-eac2b29c0ef6, Irvin Sayers
User: 31f66f05-ac9b-4723-9b5d-8f381f5a6e25, Isaiah Langer
User: 7ee95e20-247d-43ef-b368-d19d96550c81, Johanna Lorenz
User: b2fa93ac-19a0-499b-b1b6-afa76c44a301, Joni Sherman
User: 01db13c5-74fc-470a-8e45-d6d736f8a35b, Jordan Miller
User: fb0b8363-4126-4c34-8185-c998ff697a60, Lee Gu
User: ee75e249-a4c1-487b-a03a-5a170c2aa33f, Lidia Holloway
User: 5449bd61-cc63-40b9-b0a8-e83720eeefba, Lynne Robbins
User: 7ce295c3-25fa-4d79-8122-9a87d15e2438, Miriam Graham
User: 737fe0a7-0b67-47dc-b7a6-9cfc07870705, Nestor Wilke
User: a1572b58-35cd-41a0-804a-732bd978df3e, Patti Fernandez
User: 7275e1c4-5698-446c-8d1d-fa8b0503c78a, Pradeep Gupta
User: 96ab25eb-6b69-4481-9d28-7b01cf367170, Megan Bowen
User: 846327fa-e6d6-4a82-89ad-5fd313bff0cc, Alex Wilber
User: 200e4c7a-b778-436c-8690-7a6398e5fe6e, MOD Administrator
User: 7a7fded6-0269-42c2-a0be-512d58da4463, Adele Vance
User: 752f0102-90f2-4b8d-ae98-79dee995e35e,   Removed?:deleted
User: 4887248a-6b48-4ba5-bdd5-fed89d8ea6a0,   Removed?:deleted
User: e538b2d5-6481-4a90-a20a-21ad55ce4c1d,   Removed?:deleted
User: bc5994d9-4404-4a14-8fb0-46b8dccca0ad,   Removed?:deleted
User: d4e3a3e0-72e9-41a6-9538-c23e10a16122,   Removed?:deleted
```

In the Microsoft 365 Admin Portal, repeat the process of editing a user and **Save** again.

The application will receive another notification and will query the graph again using the last delta link it received. However, this time you will notice that only the modified user was returned in the results.

```shell
User: 7a7fded6-0269-42c2-a0be-512d58da4463, Adele Vance
```

Using this combination of notifications with delta query you can be assured you wont miss any updates to a resource. Notifications may be missed due to transient connection issues, however the next time your application gets a notification it will pick up all the changes since the last successful query.
