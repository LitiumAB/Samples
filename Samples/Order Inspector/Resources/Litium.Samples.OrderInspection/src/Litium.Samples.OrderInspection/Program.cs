using Litium.Samples.OrderInspection.Authentication;
using Litium.Samples.OrderInspection.Configuration;
using Litium.Samples.OrderInspection.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Use Litium username and password as HTTP Basic credentials."
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("basic", null),
            []
        }
    });
});
builder.Services
    .AddOptions<LitiumAdminApiOptions>()
    .Bind(builder.Configuration.GetSection(LitiumAdminApiOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddHttpClient<ILitiumAuthenticationService, LitiumAuthenticationService>();
builder.Services.AddHttpClient<ILitiumOrderInspectorClient, LitiumOrderInspectorClient>();
builder.Services
    .AddAuthentication(LitiumAuthenticationDefaults.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, LitiumAuthenticationHandler>(LitiumAuthenticationDefaults.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
