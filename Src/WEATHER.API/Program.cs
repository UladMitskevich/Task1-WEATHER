using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WEATHER.API.Bootstrap;
using WEATHER.API.Services.Contracts;
using WEATHER.API.Services.Implementation;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();

//one of options how to handle TableClient
builder.Services.AddSingleton<TableClient>(sp =>
{
    var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    var tableName = Environment.GetEnvironmentVariable("WeatherTableName");
    return new TableClient(storageConnectionString, tableName);
});

builder.Services.AddSingleton(sp =>
{
    var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    return new BlobServiceClient(storageConnectionString);
});

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();
builder.Services
    //Add httpClient to avoid creating a new instance for each function
    //Could be added as Fabric if needed for more complex scenarios
    .AddHttpClient()
    .AddTransient<IWeatherService,WeatherService>()
    .AddTransient<IWeatherFetcherService, WeatherFetcherService>()
    .AddTransient<IWeatherRequestPayloadService, WeatherRequestPayloadService>();

builder.Build().Run();
