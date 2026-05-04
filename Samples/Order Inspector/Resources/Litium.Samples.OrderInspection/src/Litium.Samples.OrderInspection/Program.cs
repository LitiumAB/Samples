using Litium.Samples.OrderInspection.Configuration;
using Litium.Samples.OrderInspection.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOptions<LitiumAdminApiOptions>()
    .Bind(builder.Configuration.GetSection(LitiumAdminApiOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddHttpClient<ILitiumAuthenticationService, LitiumAuthenticationService>();
builder.Services.AddHttpClient<ILitiumOrderInspectorClient, LitiumOrderInspectorClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
