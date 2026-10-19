using System.Reflection;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Events.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application.Events;

public static class EventServiceExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services, Assembly assembly)
    {
        Type[] handlerContracts = [typeof(IBeforeCommitEventHandler<>), typeof(IAfterCommitEventHandler<>)];
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

        services.AddScoped<IEventDispatcher, EventDispatcher>();

        return services;
    }
}
