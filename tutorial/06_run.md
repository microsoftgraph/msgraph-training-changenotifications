<!-- markdownlint-disable MD002 MD041 -->

### Run the application

Select **Debug > Start debugging** to run the application. After building the application a browser window will open to a 404 page. This is ok since our application is an API and not a webpage.

To subscribe for change notifications for users navigate to the following url `http://localhost:5000/api/notifications`. If successful you will see output that includes a subscription id like the one below:

```shell
Subscribed. Id: e2dbfbe1-160b-42b0-9b9f-8ab79bf8dfed
```

Your application is now subscribed to receive notifications from the Microsoft Graph when an update is made on any users in the Office 365 tenant.

Open a browser and visit the [Microsoft 365 admin center](https://admin.microsoft.com/AdminPortal). Sign-in using an administrator account. Select **Users > Active users**. Select an active user and select **Edit** for their **Contact information**. Update the **Mobile phone** value with a new number and Select **Save**.

![Screen shot of user details](./images/03.png)

In the **DEBUG CONSOLE** of Visual Studio you will see a notification has been received. Sometimes this may take a few minutes to arrive. An example of the output is below:

```shell
Received notification: 'Users/7a7fded6-0269-42c2-a0be-512d58da4463', 7a7fded6-0269-42c2-a0be-512d58da4463
```

This indicates the application successfully received the notification from the Microsoft Graph for the user specified in the output. You can then use this information to query the graph for the users full details if you want to synchronize their details into your application.