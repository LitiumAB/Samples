# Litium App Demo

A [Litium App](https://docs.litium.com/documentation/litium-apps) is a decoupled application that communicates with Litium using Web API.

This is a guide on how to set up a sample app.

## Setup

1. The project-file has a reference to the `Litium.AppManagement.Application`-package (update the package if a newer version is available). Features used from the package:

    * `AddLitiumConfiguration` in `Program.cs` - adds the additional configuration files as part of appsettings.json

    * Middleware `UseLitiumAppConfigurationExtension` in `Startup.cs` - to handle the request/response pipeline

    * `AddLitiumAppConfiguration` in `Startup.cs` - to add services

1. The `Resources/AppConfig`-folder contains configuration files that should be modified for your app. The path to the folder is configured as the enviroment variable `CONFIG_PATH` in `\Resources\src\Litium.SampleApps.LitiumAppDemo\Properties\launchSettings.json`.

    * `Hosting.json`: contains hosting information

    * `AppMetadata.json`: contains app information
