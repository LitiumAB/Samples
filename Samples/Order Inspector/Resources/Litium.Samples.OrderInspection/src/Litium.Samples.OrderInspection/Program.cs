using Litium.Samples.OrderInspection.Configuration;
using Litium.Samples.OrderInspection.LitiumApis.Generated;
using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;
using Litium.Samples.OrderInspection.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});
builder.Services
    .AddOptions<LitiumAdminApiOptions>()
    .Bind(builder.Configuration.GetSection(LitiumAdminApiOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddTransient<LitiumAccessTokenHandler>();
builder.Services.AddHttpClient<ILitiumAuthenticationService, LitiumAuthenticationService>();
builder.Services
    .AddHttpClient<ILitiumConnectErpClient, LitiumConnectErpClient>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LitiumAdminApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    })
    .AddHttpMessageHandler<LitiumAccessTokenHandler>();
builder.Services
    .AddHttpClient<ISales_sales_orderClient, Sales_sales_orderClient>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LitiumAdminApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    })
    .AddHttpMessageHandler<LitiumAccessTokenHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
