using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using TMSWebApi.Configuration;

namespace TMSWebApi.ServiceBus.Implementation;

public class AzureServiceBusConsumer : IHostedService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    
    public AzureServiceBusConsumer(IOptions<AzureServiceBusSettings> options)
    {
        var settings = options.Value;

        _client = new ServiceBusClient(settings.ConnectionString);
        _processor = _client.CreateProcessor(settings.ReplyQueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false
        });
        
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _processor.StartProcessingAsync();
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync();
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }
    
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            Console.WriteLine($"Received message: {args.Message.Body}");
            
            // Complete the message to remove it from the queue
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Message processing failed: {ex.Message}");
        }
    }

    // Event handler for handling errors
    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception.Message}");
        return Task.CompletedTask;
    }
}
