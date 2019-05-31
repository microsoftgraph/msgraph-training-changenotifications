<!-- markdownlint-disable MD002 MD041 -->

Create a launch configuration to debug the application in Visual Studio Code.

Within Visual Studio Code, select **Debug > Open Configurations**.

  ![Screencast of VS Code opening launch configurations](./images/vscode-debugapp-01.png)

When prompted to select an environment, select **.NET Core**.

  ![Screencast of VS Code creating a launch configuration for .NET Core](./images/vscode-debugapp-02.png)

> [!NOTE]
> By default, the .NET Core launch configuration will open a browser and navigate to the default URL for the application when launching the debugger. For this application, we instead want to navigate to the NGrok URL. If you leave the launch configuration as is, each time you debug the application it will display a broken page. You can just change the URL, or change the launch configuration to not launch the browser:

Update the Visual Studio debugger launch configuration:

  1. In Visual Studio Code, open the file **.vscode/launch.json**.
  1. Locate the following section in the default configuration:

      ```json
      "launchBrowser": {
        "enabled": true
      },
      ```

  1. Set the `enabled` property to `false`.
  1. Save your changes.

### Test the application:

In Visual Studio Code, select **Debug > Start debugging** to run the application. VS Code will build and star the application.

Once you see the following in the **Debug Console** window...

![Screenshot of the VS Code Debug Console](./images/vscode-debugapp-03.png)

... open a browser and navigate to **http://localhost:5000/api/notifications** to subscribe to change notifications. If successful you will see output that includes a subscription id like the one below:

![Screenshot of a successful subscription](./images/vscode-debugapp-04.png)

Your application is now subscribed to receive notifications from the Microsoft Graph when an update is made on any users in the Office 365 tenant.

Trigger a notification:

1. Open a browser and navigate to the [Microsoft 365 admin center (https://admin.microsoft.com/AdminPortal)](https://admin.microsoft.com/AdminPortal).
1. If prompted to login, sign-in using an admin account.
1. Select **Users > Active users**.

    ![Screenshot of the Microsoft 365 Admin Center](./images/vscode-debugapp-05.png)

1. Select an active user and select **Edit** for their **Contact information**.

    ![Screenshot of a user's details](./images/vscode-debugapp-06.png)

1. Update the **Phone number** value with a new number and Select **Save**.

    In the Visual Studio Code **Debug Console**, you will see a notification has been received. Sometimes this may take a few minutes to arrive. An example of the output is below:

    ```shell
    Received notification: 'Users/7a7fded6-0269-42c2-a0be-512d58da4463', 7a7fded6-0269-42c2-a0be-512d58da4463
    ```

This indicates the application successfully received the notification from the Microsoft Graph for the user specified in the output. You can then use this information to query the Microsoft Graph for the users full details if you want to synchronize their details into your application.