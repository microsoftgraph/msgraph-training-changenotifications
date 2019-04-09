<!-- markdownlint-disable MD002 MD041 -->

This tutorial teaches you how to build a .NET Core app that uses the Microsoft Graph API to receive notifications (webhooks) when a user account changes in Azure Active Directory (Azure AD) and perform queries using the delta query API to receive all changes to user accounts since the last query was made . Using this combination of notifications

> [!TIP]
> If you prefer to just download the completed tutorial, you can download or clone the [GitHub repository](https://github.com/microsoftgraph/msgraph-training-changenotifications).

## Prerequisites

Before you start this tutorial, you should have [.NET Core 2.2 SDK](https://dotnet.microsoft.com/download) and [Visual Studio Code](https://code.visualstudio.com/) installed on your development machine. If you do not have them installed, visit the previous links for download options.

> [!NOTE]
> This tutorial was written with .NET Core version 2.2. The steps in this guide may work with other versions, but that has not been tested.

## Feedback

Please provide any feedback on this tutorial in the [GitHub repository](https://github.com/microsoftgraph/msgraph-training-changenotifications).