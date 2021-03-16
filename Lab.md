# Using Change Notifications and Track Changes with Microsoft Graph

In this lab you will create a .NET Core console application that receives change notifications from the Microsoft Graph when an update is made to a users account in Azure Active Directory (Azure AD). The application will managed the Change Notification subscription and use Track Changes in the Microsoft Graph to ensure no changes are missed.

## In this lab

1. [Introduction to the lab](./tutorial/01_intro.md)
1. [Register and grant consent to the application in Microsoft Graph](./tutorial/02_create-app.md)
1. [Install ngrok](./tutorial/03_ngrok.md)
1. [Create the .NET Core project](./tutorial/04_create-project.md)
1. [Code the HTTP API](./tutorial/05_add-code.md)
1. [Run the application](./tutorial/06_run.md)
1. [Manage notification subscriptions](./tutorial/07_subscription-management.md)
1. [Query for changes](./tutorial/08_deltaquery.md)
1. [Completed Lab](./tutorial/09_completed.md)
1. [Next Steps - Use Azure EventHub to receive your notifications](./tutorial/10_EventHubs.md)

## Prerequisites

Before you start this tutorial, you should have [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download) and [Visual Studio Code](https://code.visualstudio.com/) installed on your development machine.

## Completed Exercises

Finished solutions are provided in the [Demos](./demos) folder if you get stuck.