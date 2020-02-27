# litium-auth0

**Tested in Litium version: 7.4**

Use the identityprovider [Auth0](https://auth0.com/) to login to Litium.

## About

The sample is based on logic from:
* https://auth0.com/docs/quickstart/webapp/aspnet-owin/01-login
* https://docs.litium.com/documentation/architecture/external-login-providers

## Setup in Auth0

1. Login to Auth0 and select your application

1. Add your sites domain to your Auth0-applications list of _Allowed Callback URLs_ AND _Allowed Logout URLs_

1. Copy the values for **Domain**, **Client ID** and **Client Secret** to use in Litium setup

## Setup in Litium

The installation instruction below assumes you are adding Auth0 to a Litium Accelerator installation.

1. Add the following keys to `<appSettings>` in Web.config, update with correct values:
    ```XML
    <add key="Auth0:Domain" value="yourdomain.eu.auth0.com" />
    <add key="Auth0:ClientId" value="TODO" />
    <add key="Auth0:ClientSecret" value="TODO" />
    <!-- Where to redirect the user after Auth0-login -->
    <add key="Auth0:RedirectUri" value="https://mytestsite.localtest.me" />
    <!-- Where to redirect the user after Auth0-logout -->
    <add key="Auth0:PostLogoutRedirectUri" value="https://mytestsite.localtest.me" />
    <!-- Person fieldtemplate used when creating new persons upon first login -->
    <add key="Auth0:PersonTemplate" value="SystemUserTemplate" />
    ```

1. Add the following NuGet-packages to the `Litium.Accelerator`-project
    ```console
    Install-Package Microsoft.AspNet.Identity.Core
    Install-Package Microsoft.Owin.Security.OpenIdConnect
    Install-Package Microsoft.Owin.Host.SystemWeb
    ```

1. Add the following NuGet-package to the `Litium.Accelerator.Mvc`-project (to generate proper binding-redirects)
    ```console
    Install-Package Microsoft.Owin.Security.OpenIdConnect
    ```

1. Copy the folder `Resources/Auth0` with all its files to the _Litium.Accelerator_-project in your solution

1. Copy the file `Resources/Auth0LoginController.cs` to `\Src\Litium.Accelerator.Mvc\Controllers\Login` - remember to also include the file in the project

1. According to [the instructions on docs](https://docs.litium.com/documentation/architecture/external-login-providers), add the snippet below to the method `TryGet()` in _\Src\Litium.Accelerator.Mvc\Routing\LoginPageSignInUrlResolverDecorator.cs_:
    ```C#
    public bool TryGet(RouteRequestLookupInfo routeRequestLookupInfo, out string redirectUrl)
    {
        if (HttpContext.Current.SkipAuthorization)
        {
            redirectUrl = null;
            return false;
        }

        ...
    ```

## Use

Add the snippet below where you want to place a loginbutton that switch between login/logout functionality:

```HTML+Razor
@if (User.Identity.IsAuthenticated)
{
    @Html.ActionLink($"Log out {User.Identity.Name}", "Logout", "Auth0Login", null, new { @class = "profile__link--block" })
}
else
{
    @Html.ActionLink("Login with Auth0", "RedirectToProvider", "Auth0Login", null, new { @class = "profile__link--block" })
}
```
This could for example be in `\Src\Litium.Accelerator.Mvc\Views\Shared\Framework\Profile.cshtml` to render the button together with the _login/my page_-link of the Accelerator.

The regular Log-out button of MyPage will also work fine to log out.

Load your page and click the Auth0 login button, this should open a popup where you can login with your Auth0-account.

> If you get an error similar to this: _"Could not load file or assembly 'Microsoft.Owin.Host.SystemWeb, Version=4.1.0.0"_ - you may need to consolidate nuget-packages to use the same most recent version for that package:
>
>   1. Open Tools > NuGet package manager > Manage NuGet packages for solution...
>   1. Select the consolidate tab
>   1. Verify that the package throwing the error is on the same latest version for all projects where it is used
>
