<!-- markdownlint-disable MD002 MD041 -->

### Create the .NET Core project

Open your command prompt, navigate to a directory where you have rights to create files, and run the following commands to create a new .NET Core WebApi app.

```shell
dotnet new webapi -o msgraphapp
```

Once the command finishes, run the following commands to ensure your new project runs correctly.

```shell
cd msgraphapp
dotnet add package Microsoft.Identity.Client
dotnet add package Microsoft.Graph
dotnet run
```

The application will start and output:

```shell
Now listening on: http://localhost:5000
```

If you don't see this output, or you see error messages, there is likely a problem with the [.NET Core 2.2 SDK](https://dotnet.microsoft.com/download) installed on your development machine that needs to be fixed before continuing.

Stop the application running by pressing <kbd>CTRL</kbd>+<kbd>C</kbd>.

Open the application in Visual Studio Code using the following command:

```shell
code .
```

When a dialog box asks if you want to add required assets to the project, select **Yes**.
