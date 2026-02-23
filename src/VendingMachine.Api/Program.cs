using VendingMachine.Api.Configuration;
using VendingMachine.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddModules(builder.Configuration);

var app = builder.Build();

app.UseApiExceptionHandler();
app.MapApiEndpoints();

app.Run();
