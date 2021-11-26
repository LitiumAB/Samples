# litium-auth0

**Tested in Litium version: 8.1.0**

Use the identityprovider [Auth0](https://auth0.com/) to login to Litium administration.

## About

The sample is based on logic from:
* https://auth0.com/docs/quickstart/webapp/aspnet-core-2/01-login

## Setup in Auth0

1. Login to Auth0 and select your application

1. Add your sites domain to your Auth0-applications list of _Allowed Callback URLs_ AND _Allowed Logout URLs_

1. Copy the values for **Domain**, **Client ID** and **Client Secret** to use in Litium setup

## Setup in Litium

The installation instruction below assumes you are adding Auth0 to a Litium Accelerator installation.

1. Configure the Auth0 OpenIdConnect provider with the configuration, update with correct values:
    ```json
    {
        "Auth0": {
            "Domain": "yourdomain.eu.auth0.com",
            "ClientId": "TODO",
            "ClientSecret": "TODO",
            "RedirectUri": "/", // "Where to redirect the user after Auth0-logout"
            "PersonTemplate": "_system" // "Person fieldtemplate used when creating new persons upon first login"
        }
    }
    ```

1. Configure Litium for the alternative administration sign in and out urls
    ```json
    {
        "Litium": {
            "AdministrationSecurity" : {
                "RequiredClaims": {
                    "provider": "auth0"
                },
                "SignInUrl": "/auth0-authentication/login?redirectUrl={redirect_url}",
                "SignOutUrl": "/auth0-authentication/logout"
            }
        }
    }
    ```

1. Copy the folder `Auth0` from `Resources` with all its files to the _Litium.Accelerator.Mvc_-project in your solution
