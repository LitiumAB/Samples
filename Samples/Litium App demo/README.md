# Litium App Demo

**Tested in Litium version: Litium 8 beta**

Introducing Litium App - a decoupled application that communicates with Litium using web API.

This is the guide on how to develop an application as Litium App.

## Instructions

1. Install latest `Litium.AppManagement.Application` package to application. 
    ```
    <ItemGroup>
    <PackageReference Include="Litium.AppManagement.Application" Version="1.0.0" />
    </ItemGroup>
    ```

2. `AppConfig` folder contains sample configuration files for a Litium App.
    - `Hosting.json`: contains information of hosting.
    - `AppMetadata.json`: contains information of Litium App.

3. Set `AppConfig` folder path to enviroment variable `CONFIG_PATH`.  
In the sample, it has been set in `launchSettings.json`

4. In the `Program.cs`, add `LitiumConfiguration`.
    ```
    .ConfigureHostConfiguration(config =>
    {
        config.AddLitiumConfiguration();
        config.AddEnvironmentVariables();
    })
    ```
5. In the `ConfigureServices` method of `Startup.cs`, register service of Litium App
    ```
    services.AddLitiumAppConfiguration(Configuration, opt =>
    {
        opt.ShutdownOnConfigChanges = true;
        opt.ValidateCertificate = false;
    });
    ```

6. In the `Configure` method, insert middleware to use Litium App
    ```
    app.UseLitiumAppConfigurationExtension();
    ```