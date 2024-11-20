using TMSWebApi.ServiceBus.Implementation;
using TMSWebApi.ServiceBus.Interfaces;

namespace TMSWebApi.Configuration;

public static class InitializeServicesExtension
{
    public static void InitializeServices(this IServiceCollection services)
    {
        services.AddSingleton<IServiceBusHandler, ServiceBusHandler>();
    }
}