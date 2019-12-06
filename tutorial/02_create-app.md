<!-- markdownlint-disable MD002 MD041 -->

In this exercise, you will create a new Azure AD web application registration using the Azure Active Directory admin center and grant administrator consent to the required permission scopes.

1. Open a browser and navigate to the [Azure Active Directory admin center (https://aad.portal.azure.com)](https://aad.portal.azure.com). Login using a **Work or School Account**.

1. Select **Azure Active Directory** in the left-hand navigation, then select **App registrations** under **Manage**.

    ![Screenshot of the App registrations](./images/aad-portal-home.png)

1. Select **New registration**. On the **Register an application** page

    ![Screenshot of App Registrations page](./images/aad-portal-newapp.png)

    Set the values as follows:

    - **Name**: GraphNotificationTutorial
    - **Supported account types**: Accounts in any organizational directory and personal Microsoft accounts
    - **Redirect URI**: Web > http://localhost

    ![Screenshot of the Register an application page](./images/aad-portal-newapp-01.png)

    Select **Register**.

1. On the **GraphNotificationTutorial** page, copy the value of the **Application (client) ID** and **Directory (tenant) ID** save it, you will need them later in the tutorial.

    ![Screenshot of the application ID of the new app registration](./images/aad-portal-newapp-details.png)

1. Select **Manage > Certificates & secrets**. 

    Select **New client secret**.

    Enter a value in **Description** and select one of the options for **Expires** and select **Add**.

    ![Screenshot of the Add a client secret dialog](./images/aad-portal-newapp-secret.png)

    ![Screenshot of the Add a client secret dialog](./images/aad-portal-newapp-secret-02.png)

1. Copy the client secret value before you leave this page. You will need it later in the tutorial.

    > [!IMPORTANT]
    > This client secret is never shown again, so make sure you copy it now.

    ![Screenshot of the newly added client secret](./images/aad-portal-newapp-secret-03.png)

1. Select **Manage > API Permissions**.

    Select **Add a permission** and select **Microsoft Graph**.

    ![Screenshot selecting the Microsoft Graph service](./images/aad-portal-newapp-graphscope.png)

    Select **Application Permission**, expand the **User** group and select **User.Read.All** scope.

    Select **Add permissions** to save your changes.

    ![Screenshot selecting Microsoft Graph scope](./images/aad-portal-newapp-graphscope-02.png)

    ![Screenshot of the newly added client secret](./images/aad-portal-newapp-graphscope-03.png)

The application requests an application permission with the **User.Read.All** scope. This permission requires administrative consent.

Select **Grant admin consent for [your tenant name]**, then select **Yes** to consent this application and grant the application access to your tenant using the scopes you specified.

![Screenshot approved admin consent](./images/aad-portal-newapp-graphscope-04.png)
