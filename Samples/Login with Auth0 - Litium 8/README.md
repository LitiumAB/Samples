# litium-auth0

**Tested in Litium version: 8.19.1**

Use the identityprovider [Auth0](https://auth0.com/) to login to Litium administration.

## About

The sample is based on logic from:

- https://auth0.com/docs/quickstart/webapp/aspnet-core/interactive

## Setup in Auth0

1. Login to Auth0 and select your application (ASP.NET Core med MVC for both React and MVC accelerator)

1. Add your sites domain to your Auth0-applications list of _Allowed Callback URLs_ AND _Allowed Logout URLs_ (MVC domain for MVC accelerator and React domain for React accelerator)

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
       "RedirectUri": "/", // "Where to redirect the user after Auth0-logout, if you are using domain with prefixes logout redirect end up to the domain without prefix"
       "PersonTemplate": "_system" // "Person fieldtemplate used when creating new persons upon first login"
     }
   }
   ```

1. Configure Litium for the alternative administration sign in and out urls

   ```json
   {
     "Litium": {
       "AdministrationSecurity": {
         "RequiredClaims": {
           "provider": "auth0"
         },
         "SignInUrl": "/auth0-authentication/login?redirectUrl={redirect_url}",
         "SignOutUrl": "/auth0-authentication/logout"
       }
     }
   }
   ```

1. Configure allowed paths if you are using react accelerator.

   ```json
   {
     "Litium": {
       "DevProxy": {
         "Items": [
           {
             "Name": "Auth0 end points",
             "Path": "auth0-authentication/{**remainder}"
           },
           {
             "Name": "Auth0Callback",
             "Path": "auth0-callback"
           },
           {
             "Name": "Auth0SignoutCallback",
             "Path": "auth0-signout-callback"
           }
         ]
       }
     }
   }
   ```

1. Copy the folder `Auth0` from `Resources` with all its files to the _Litium.Accelerator.Mvc_-project in your solution if you are using MVC accelerator. If you are using React accelerator copy the folder to your Litium Empty project.
