using GroceryListAi.Core.Services;

namespace GroceryListAi.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructuralServices(this IServiceCollection services)
    {
        var serviceTypes = typeof(ItemService).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(ItemService).Namespace
                && type is { IsClass: true, IsAbstract: false, IsNested: false });

        foreach (var serviceType in serviceTypes)
        {
            var serviceInterface = serviceType.GetInterface($"I{serviceType.Name}");

            if (serviceInterface is null)
                services.AddScoped(serviceType);
            else
                services.AddScoped(serviceInterface, serviceType);
        }

        return services;
    }
}
