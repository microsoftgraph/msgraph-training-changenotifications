<!-- markdownlint-disable MD002 MD041 -->

Open your command prompt, navigate to a directory where you have rights to create your project, and run the following command to create a new .NET Core WebApi app:

```shell
dotnet new webapi -o msgraphapp
```

After creating the application, run the following commands to ensure your new project runs correctly.

  ```shell
  cd msgraphapp
  dotnet add package Microsoft.Identity.Client
  dotnet add package Microsoft.Graph
  dotnet run
  ```

  The application will start and output the following:

  ```shell
  info: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[0] ...
  info: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[58] ...
  warn: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[35] ...
  info: Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository[39] ...
  Hosting environment: Development
  Content root path: /Users/ac/_play/graphchangenotifications/msgraphapp
  Now listening on: https://localhost:5001
  Now listening on: http://localhost:5000
  Application started. Press Ctrl+C to shut down.
  ```

Stop the application running by pressing <kbd>CTRL</kbd>+<kbd>C</kbd>.

Open the application in Visual Studio Code using the following command:

```shell
code .
```

If Visual Studio code displays a dialog box asking if you want to add required assets to the project, select **Yes**.
