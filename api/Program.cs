using BrokerPulse.Api.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// No UseHttpsRedirection(): Azure Container Apps ingress terminates TLS and forwards
// plain HTTP to this container's target port, so redirecting here loops.

app.MapEndpointGroups();

app.Run();
