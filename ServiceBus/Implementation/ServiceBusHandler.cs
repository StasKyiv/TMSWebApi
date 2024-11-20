using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using TMSWebApi.Configuration;
using TMSWebApi.DTOs;
using TMSWebApi.ServiceBus.Interfaces;

namespace TMSWebApi.ServiceBus.Implementation;

public class ServiceBusHandler : IServiceBusHandler
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _senderTaskQueue;
    private string _taskQueueName;
    private string _replyQueueName;
    private string _getListTaskQueueName;

    public ServiceBusHandler(IOptions<AzureServiceBusSettings> options)
    {
        _client = new ServiceBusClient(options.Value.ConnectionString);
        _taskQueueName = options.Value.QueueName;
        _getListTaskQueueName = options.Value.GetQueueName;
        _replyQueueName = options.Value.ReplyQueueName;
        _senderTaskQueue = _client.CreateSender(_taskQueueName);
    }

    // Method to send messages
    public async Task SendMessageAsync(string type, WorkTaskDto? workTask)
    {
        try
        {
            var message = new ServiceBusMessage();
            if (type is "post" or "put")
            {
                var body = JsonSerializer.Serialize(workTask);
                message = new ServiceBusMessage(body);
            }

            message.MessageId = Guid.NewGuid().ToString();
            message.ApplicationProperties.Add("MessageType", type);
            message.ApplicationProperties.Add("IsResponse", false);

            await _senderTaskQueue.SendMessageAsync(message);
            Console.WriteLine($"Message was sent: {message.MessageId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendMessageAsync error: {ex.Message}");
        }
    }
    
    public async Task<string> ReceiveCreatedUpdateTaskResponseAsync(string type)
    {
        try
        {
            string messageBody = null;
            var processor = _client.CreateProcessor(type is "post" ? _taskQueueName : _getListTaskQueueName);

            processor.ProcessMessageAsync += async args =>
            {
                messageBody = args.Message.Body.ToString();
                await args.CompleteMessageAsync(args.Message);
            };

            // Set up the ProcessErrorAsync handler to handle any errors
            processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine($"Error receiving message: {args.Exception.Message}");
            };

            await processor.StartProcessingAsync();
            await Task.Delay(5000);
            await processor.StopProcessingAsync();

            return messageBody;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
            return null;
        }
    }
    public async Task<MessageDto> ReceiveUpdateTaskResponseAsync()
    {
        try
        {
            var receivedTasks = new MessageDto();

            var processor = _client.CreateProcessor(_replyQueueName);
            
            processor.ProcessMessageAsync += async args =>
            {
                var messageBody = args.Message.Body.ToString();
                receivedTasks = JsonSerializer.Deserialize<MessageDto>(messageBody);
                
                await args.CompleteMessageAsync(args.Message);
            };
            
            processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine($"Error receiving message: {args.Exception.Message}");
            };

            await processor.StartProcessingAsync();
            await Task.Delay(5000);
            await processor.StopProcessingAsync();

            return receivedTasks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
            return null;
        }
    }
}