using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application.Events;

public static class EventHandlersServiceExtensions
{
    public static IServiceCollection AddEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerContracts = new[] { typeof(IBeforeCommitEventHandler<>), typeof(IAfterCommitEventHandler<>) };
        var registrations = assembly.GetTypes()
            .Where(type => !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces(), (type, contract) => (type, contract))
            .Where(pair => pair.contract.IsGenericType)
            .Where(pair => handlerContracts.Contains(pair.contract.GetGenericTypeDefinition()))
            .ToList();

        foreach (var (type, contract) in registrations)
        {
            services.AddTransient(contract, type);
        }

        return services;
    }
}
