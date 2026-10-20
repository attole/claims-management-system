using System.Reflection;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Events.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application.Events;

public static class EventServiceExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services, Assembly assembly)
    {
        var classes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .ToList();
        var beforeCommitHandlers = classes.Where(type => typeof(IBeforeCommitEventHandler).IsAssignableFrom(type));
        var afterCommitHandlers = classes.Where(type => typeof(IAfterCommitEventHandler).IsAssignableFrom(type));

        foreach (var handler in beforeCommitHandlers)
        {
            services.AddTransient(typeof(IBeforeCommitEventHandler), handler);
        }

        foreach (var handler in afterCommitHandlers)
        {
            services.AddTransient(typeof(IAfterCommitEventHandler), handler);
        }

        services.AddScoped<IEventDispatcher, EventDispatcher>();

        return services;
    }
}
