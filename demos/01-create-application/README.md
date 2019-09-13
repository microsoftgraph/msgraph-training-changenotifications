# Completed module: Create and run the .NET Core project

The version of the project in this directory reflects completing the tutorial up through [Run the application](../../tutorial/06_run.md). If you use this version of the project, you need to complete the rest of the tutorial starting at [Manage notification subscriptions](../../tutorial/07_subscription-management.md).

> **Note:** It is assumed that you have already registered an application in the app registration portal as specified in [Register and grant consent to the application in Microsoft Graph](../../tutorial/02_create-app.md). You need to configure this version of the sample as follows:
>
> 1. Rename the file **appsettings.json.example** to **appsettings.json**
> 1. Edit the **appsettings.json** file and make the following changes.
>     1. `<NGROK URL>` should be set to the ngrok https forwarding address. See [Install ngrok](../../tutorial/03_ngrok.md) for information on installing and running ngrok.
>     1. `<TENANT ID>` should be your Office 365 tenant id you got from the App Registration Portal.
>     1. `<APP ID>` and `<APP SECRET>` should be the application id and secret you got from the App Registration Portal.
