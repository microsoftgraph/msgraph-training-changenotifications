---
uid: dotnet-webhooks-eventhubs-sample
description: Learn how to receive change notifications via Event Hubs
page_type: sample
createdDate: 03/10/2021 00:00:00 AM
languages:
- csharp
technologies:
  - Microsoft Graph
  - Microsoft identity platform
authors:
- id: kkrishna
  displayName: Kalyan Krishna
products:
- ms-graph
- dotnet-core
extensions:
  contentType: samples
  technologies: 
    - Microsoft Graph
    - Microsoft identity platform
  createdDate: 03/10/2021
codeUrl: https://github.com/microsoftgraph/msgraph-training-changenotifications
zipUrl: https://github.com/microsoftgraph/msgraph-training-changenotifications/master/master.zip
description: "This sample demonstrates a .NET console application showcasing Microsoft Graph change notifications delivered via Azure Event Hubs"
---
# Microsoft Graph change notifications delivered via Azure Event Hubs

- [Microsoft Graph change notifications delivered via Azure Event Hubs](#microsoft-graph-change-notifications-delivered-via-azure-event-hubs)
  - [Overview](#overview)
  - [Prerequisites](#prerequisites)
  - [Registration](#registration)
    - [Step 1: Register your application](#step-1-register-your-application)
    - [Step 2: Set the MS Graph permissions](#step-2-set-the-ms-graph-permissions)
  - [Using Azure Event Hubs to receive change notifications](#using-azure-event-hubs-to-receive-change-notifications)
    - [Set up the Azure KeyVault and Azure Event Hubs](#set-up-the-azure-keyvault-and-azure-event-hubs)
      - [Configuring the Azure Event Hub](#configuring-the-azure-event-hub)
      - [Configuring the Azure Key Vault](#configuring-the-azure-key-vault)
        - [What happens if the Microsoft Graph change tracking application is missing?](#what-happens-if-the-microsoft-graph-change-tracking-application-is-missing)
      - [Configuring the Azure Storage Account](#configuring-the-azure-storage-account)
    - [Creating the subscription and receiving notifications](#creating-the-subscription-and-receiving-notifications)
  - [Setup](#setup)
    - [Step 1:  Clone or download this repository](#step-1--clone-or-download-this-repository)
  - [Configure the sample](#configure-the-sample)
    - [NotificationUrl](#notificationurl)
    - [EventHubConnectionString](#eventhubconnectionstring)
    - [EventHubName](#eventhubname)
    - [StorageConnectionString](#storageconnectionstring)
    - [BlobContainerName](#blobcontainername)
    - [TenantDomain](#tenantdomain)
  - [Run the sample](#run-the-sample)
    - [On Visual Studio](#on-visual-studio)
    - [Using the app](#using-the-app)

## Overview

This sample helps you explore the Microsoft Graph's [change notification delivery via Azure Event Hubs](https://docs.microsoft.com/graph/change-notifications-delivery).

Change notifications can be delivered in different ways to subscribers. If the main delivery mode for change notifications is through webhooks, it can be challenging to take advantage of webhooks for high throughput scenarios or when the receiver cannot expose a publicly available notification URL.  

Receiving change notifications via Azure Event Hubs as the delivery mode is available for all resources that support Microsoft Graph change notifications.

Good examples of high throughput scenarios include applications subscribing to a large set of resources, applications subscribing to resources that change with a high frequency, and multi-tenant applications that subscribe to resources across a large set of organizations.

## Prerequisites

- Either [Visual Studio](https://aka.ms/vsdownload) *or* [Visual Studio Code](https://code.visualstudio.com/) with [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) and [C# for Visual Studio Code Extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
- An Azure Active Directory (Azure AD) tenant. For more information, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/)
- [An Azure subscription](https://azure.microsoft.com/free/).
- A user account in your Azure AD tenant. This sample will not work with a personal Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

## Registration

### Step 1: Register your application

Use the [Microsoft Application Registration Portal](https://aka.ms/appregistrations) to register your application with the Microsoft Microsoft Identity Platform.

![Application Registration](docs/register_app.png)
**Note:** Make sure to set the right **Redirect URI** (`http://localhost`) and application type is **Mobile and desktop applications**.

In the app's registration **Overview** screen, find and note the **Application (client) ID** and **Directory (tenant) ID**. You use this value in your app's configuration file(s) later in your code.

### Step 2: Set the MS Graph permissions

Add the [delegated permissions](https://docs.microsoft.com/graph/permissions-reference#delegated-permissions-20) for `Directory.Read.All`, `User.ReadWrite.All`. We advise you to register and use this sample on a Dev/Test tenant and not on your production tenant.

![Api Permissions](docs/api_permissions.png)

## Using Azure Event Hubs to receive change notifications

[Azure Event Hubs](https://azure.microsoft.com/services/event-hubs) is a popular real-time events ingestion and distribution service built for scale. You can use Azure Events Hubs instead of traditional webhooks to receive change notifications. This feature is currently in preview.  
Using Azure Event Hubs to receive change notifications differs from webhooks in a few ways, including:

- You don't rely on publicly exposed notification URLs. The Event Hubs SDK will relay the notifications to your application.
- You don't need to reply to the [notification URL validation](webhooks.md#notification-endpoint-validation). You can ignore the validation message that you receive.
- You'll need to provision an Azure Event Hub.
- You'll need to provision an Azure Key Vault.
- You'll need to provision an Azure blob container.

### Set up the Azure KeyVault and Azure Event Hubs

This section will walk you through the setup of required Azure services.

#### Configuring the Azure Event Hub

In this section you will:

- Create an Azure Event Hub namespace.
- Add a hub to that namespace that will relay and deliver notifications.
- Add a shared access policy that will allow you to get a connection string to the newly created hub.

Steps:

1. Open a browser to the [Azure Portal](https://portal.azure.com).
1. Select **Create a resource**.
1. Type **Event Hubs** in the search bar.
1. Select the **Event Hubs** suggestion. The Event Hubs creation page will load.  
1. On the Event Hubs creation page, click **Create**.
1. Fill in the Event Hubs namespace creation details, and then click **Create**.  
1. When the Event Hub namespace is provisioned, go to the page for the namespace.  
1. Click **Event Hubs** and **+ Event Hub**.  
1. Give a name to the new Event Hub, and click **Create**.  
1. After the Event Hub has been created, click the **name of the Event Hub**, and then click **Shared access policies** and **+ Add** to add a new policy.  
1. Give a name to the policy, check **Send**, and click **Create**.  
1. After the policy has been created, click the name of the policy to open the details panel, and then copy the **Connection string-primary key** value. Write it down; you'll need it for the next step.  

#### Configuring the Azure Key Vault

In order to access the Event Hub securely and to allow for key rotations, Microsoft Graph gets the connection string to the Event Hub through Azure Key Vault.  
In this section, you will:

- Create an Azure Key Vault to store secret.
- Add the connection string to the Event Hub as a secret.
- Add an access policy for Microsoft Graph to access the secret.

Steps:

1. Open a browser to the [Azure Portal](https://portal.azure.com).
1. Select **Create a resource**.
1. Type **Key Vault** in the search bar.
1. Select the **Key Vault** suggestion. The Key Vault creation page will load.
1. On the Key Vault creation page, click **Create**.  
1. Fill in the Key Vault creation details, and then click **Review + Create** and **Create**.  
1. Go to the newly crated key vault using the **Go to resource** from the notification.  
1. Copy the **Vault URI**; you will need it for the next step.  
1. Go to **Secrets** and click **+ Generate/Import**.  
1. Give a name to the secret, and keep the name for later; you will need it for the next step. For the value, paste in the connection string you generated at the Event Hubs step. Click **Create**.  
1. Click **Access Policies** and **+ Add Access Policy**.  
1. For **Secret permissions**, select **Get**, and for **Select Principal**, select **Microsoft Graph Change Tracking**. Click **Add**.  

##### What happens if the Microsoft Graph change tracking application is missing?

It's possible that the **Microsoft Graph Change Tracking** service principal is missing from your tenant, depending on when the tenant was created and administrative operations. To resolve this issue, run [the following query](https://developer.microsoft.com/en-us/graph/graph-explorer?request=servicePrincipals&method=POST&version=v1.0&GraphUrl=https://graph.microsoft.com&requestBody=eyJhcHBJZCI6IjBiZjMwZjNiLTRhNTItNDhkZi05YTgyLTIzNDkxMGM0YTA4NiJ9) in [Microsoft Graph Explorer](https://developer.microsoft.com/en-us/graph/graph-explorer).

Query details:

```http
POST https://graph.microsoft.com/v1.0/servicePrincipals
{
    "appId": "0bf30f3b-4a52-48df-9a82-234910c4a086"
}
```

> **Note:** You can get an access denied running this query. In this case, select the gear icon next to your account name in the top left corner. Then select **Select Permissions** and search for **Application.ReadWrite.All**. Check the permission and select **Consent**. After consenting to this new permission, run the request again.

> **Note:** This API only works with a school or work account, not with a personal account. Make sure that you are signed in with an account on your domain.

Alternatively, you can use this [Azure Active Directory PowerShell](/powershell/azure/active-directory/install-adv2) script to add the missing service principal.

```PowerShell
Connect-AzureAD -TenantId <tenant-id>
# replace tenant-id by the id of your tenant.
New-AzureADServicePrincipal -AppId 0bf30f3b-4a52-48df-9a82-234910c4a086
```

#### Configuring the Azure Storage Account

In order to setup an event hub listener, an azure storage account is required. The following storage properties are required for the event hub listener

- Storage account name
- Storage Access Key

To create a general-purpose storage account in the Azure portal, follow these steps:

1. On the [Azure Portal](https://portal.azure.com) menu, select All services. In the list of resources, type Storage Accounts. As you begin typing, the list filters based on your input. Select Storage Accounts.
1. On the Storage Accounts window that appears, choose Add.
1. On the Basics tab, select the subscription in which to create the storage account.
1. Under the Resource group field, select your desired resource group, or create a new resource group. For more information on Azure resource groups, see Azure Resource Manager overview.
1. Next, enter a name for your storage account. The name you choose must be unique across Azure. The name also must be between 3 and 24 characters in length, and may include only numbers and lowercase letters.
1. Select a location for your storage account, or use the default location.
1. Select a performance tier. The default tier is Standard.
1. Set the Account kind field to Storage V2 (general-purpose v2).
1. Specify how the storage account will be replicated. The default replication option is Read-access geo-redundant storage (RA-GRS). For more information about available replication options, see Azure Storage redundancy.
1. Additional options are available on the Networking, Data protection, Advanced, and Tags tabs. To use Azure Data Lake Storage, choose the Advanced tab, and then set Hierarchical namespace to Enabled. For more information, see Azure Data Lake Storage Gen2 Introduction
1. Select Review + Create to review your storage account settings and create the account.
1. Select Create.
1. After deployment of the resource, click view resource and go to the Access Keys menu. 
1. Click the `show keys` button and copy the Key1 `key` value for usage in the Event hub listener configuration.


### Creating the subscription and receiving notifications

After you create the required Azure KeyVault and Azure Event Hubs services, you will be able to create your subscription and start receiving change notifications via Azure Event Hubs.

## Setup

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/microsoftgraph/msgraph-training-changenotifications.git
cd demos\04-azure-eventhub-listener
```

or download and extract the repository .zip file.

## Configure the sample

In the `appSettings.json` section, populate the following keys

### NotificationUrl

You must set it to `EventHub:https://<azurekeyvaultname>.vault.azure.net/secrets/<secretname>?tenantId=<domainname>`, with the following values:

- `azurekeyvaultname` - The name you gave to the key vault when you created it. Can be found in the Vault URI.
- `secretname` - The name you gave to the secret when you created it. Can be found on the Azure Key Vault **Secrets** page.
- `domainname` - The name of your tenant; for example, contoso.onmicrosoft.com or contoso.com. Because this domain will be used to access the Azure Key Vault, it is important that it matches the domain used by the Azure subscription that holds the Azure Key Vault. To get this information, you can go to the overview page of the Azure Key Vault you created and click the subscription. The domain name is displayed under the **Directory** field.
  
### EventHubConnectionString

The connection string to your event hub in the form of :

`Endpoint=sb://<my event hub namespace>.servicebus.windows.net/;SharedAccessKeyName=<SH key name>;SharedAccessKey=<SA key>`

### EventHubName

Provide the name of the event hub that you created earlier

### StorageConnectionString

The Storage account connection string in the form of :
`DefaultEndpointsProtocol=https;AccountName=<My account name>;AccountKey=<my account key>;EndpointSuffix=core.windows.net`

### BlobContainerName

The name of the blob container you created in the storage account above
  
### TenantDomain

The Tenant/Directory domain, like contoso.onmicrosoft.com

## Run the sample

### On Visual Studio

Press F5. This will restore the missing nuget packages, build the solution and run the project.

### Using the app

If everything was configured correctly, you should be able to see the first login prompt.

Once started the app will first read the existing messages on the event hub abd list those out.

After that, the code will list all users, and then create and delete a user in this dev/test tenant.

Finally, the code will read the new messages in the event hub again. you'd notice that the following messages are listed

1. The first one is a notification validation message, that you can ignore
1. The second one is about a newly created user
1. The final one is about the deleted user.
