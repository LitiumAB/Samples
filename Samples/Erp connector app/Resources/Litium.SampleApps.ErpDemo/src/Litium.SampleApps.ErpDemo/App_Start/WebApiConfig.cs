using Microsoft.AspNet.WebHooks.Config;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Litium.SampleApps.ErpDemo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Initialize Custom WebHook receiver
            WebHooksConfig.Initialize(config);

            config.EnableSwagger(c => {
                c.ApiKey("swagger");
                c.PrettyPrint();
                c.SingleApiVersion("V1", "Litium Apps");
                c.DocumentFilter<AppDocFilter>();
                c.DescribeAllEnumsAsStrings();
                c.ResolveConflictingActions(apiDescriptions =>
                {
                    return apiDescriptions.FirstOrDefault();
                });
                c.UseFullTypeNameInSchemaIds();
                c.DescribeAllEnumsAsStrings();
            })
             .EnableSwaggerUi("apps-docs/{*assetPath}", u => { u.DisableValidator(); });
        }

        private class AppDocFilter : IDocumentFilter
        {
            private static List<string> _appRootPrefixes = new List<string> { "/rmas/", "/orders/", "/inventories/", "/prices/" };
            public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
            {
                var pathsToRemove = swaggerDoc.paths.Where(path => !_appRootPrefixes.Any(prefix => path.Key.StartsWith(prefix))).ToList();
                foreach (var path in pathsToRemove)
                {
                    swaggerDoc.paths.Remove(path);
                }
            }
        }
    }
}
