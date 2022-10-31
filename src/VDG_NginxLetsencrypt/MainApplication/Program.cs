using EngineFramework.Microsoft.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAllJsonFiles();

builder.Host.UseLamarAndPluginEngine((context, services) =>
{
});

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.WriteTo.Console();
    // configuration.MinimumLevel.Debug();
    // var logLevel = context.Configuration["Logging:LogLevel:Default"];
    // Console.WriteLine($"Logging:LogLevel:Default {context.Configuration["Logging:LogLevel:Default"]}");
    // if(string.IsNullOrEmpty(logLevel))
    //     switch (logLevel.ToLower())
    //     {
    //         case "debug":
    //             configuration.MinimumLevel.Debug();
    //             break;
    //     }
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();