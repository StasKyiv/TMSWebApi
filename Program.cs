using System.Text.Json;
using TMSWebApi.Configuration;
using TMSWebApi.DTOs;
using TMSWebApi.ServiceBus.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
    {
        p.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod()
            .WithOrigins();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.InitializeServices();

builder.Services.Configure<AzureServiceBusSettings>(
    builder.Configuration.GetSection("AzureServiceBus"));

// builder.Services.AddHostedService<AzureServiceBusConsumer>();
builder.Services.InitializeServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/", async (IServiceBusHandler serviceBusHandler, WorkTaskDto taskDto) =>
    {
        await serviceBusHandler.SendMessageAsync("post", taskDto);
        
        var result = await serviceBusHandler.ReceiveCreatedUpdateTaskResponseAsync("post");
            
        return Results.Ok(result);
    })
    .WithName("CreateWorkTask")
    .WithOpenApi();

app.MapPut("/", async (IServiceBusHandler serviceBusHandler, WorkTaskDto taskDto) =>
    {
        await serviceBusHandler.SendMessageAsync("put", taskDto);
        
        var result = await serviceBusHandler.ReceiveUpdateTaskResponseAsync();

        return result.StatusCode == 200 ? Results.Ok("Message sent successfully!"):
            Results.Problem($"Error retrieving messages: {result.Message}");
    })
    .WithName("UpdateWorkTask")
    .WithOpenApi();

app.MapGet("/", async (IServiceBusHandler serviceBusHandler) =>
    {
        await serviceBusHandler.SendMessageAsync("get", null);

        var result = await serviceBusHandler.ReceiveCreatedUpdateTaskResponseAsync("get");

        return Results.Ok(result);
    })
    .WithName("GetWorkTaskList")
    .WithOpenApi();

app.Run();