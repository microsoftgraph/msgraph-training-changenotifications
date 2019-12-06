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
  info: Microsoft.Hosting.Lifetime[0]
        Now listening on: https://localhost:5001
  info: Microsoft.Hosting.Lifetime[0]
        Now listening on: http://localhost:5000
  info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Development
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: [your file path]\msgraphapp
  ```

Stop the application running by pressing <kbd>CTRL</kbd>+<kbd>C</kbd>.

Open the application in Visual Studio Code using the following command:

```shell
code .
```

If Visual Studio code displays a dialog box asking if you want to add required assets to the project, select **Yes**.
