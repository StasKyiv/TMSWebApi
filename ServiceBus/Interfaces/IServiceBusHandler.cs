using TMSWebApi.DTOs;

namespace TMSWebApi.ServiceBus.Interfaces;

public interface IServiceBusHandler
{
    Task SendMessageAsync(string type, WorkTaskDto? workTask);
    Task<string> ReceiveCreatedUpdateTaskResponseAsync(string type);
    Task<MessageDto> ReceiveUpdateTaskResponseAsync();
}