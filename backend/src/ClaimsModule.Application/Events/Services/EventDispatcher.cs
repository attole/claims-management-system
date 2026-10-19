using System.Reflection;
using ClaimsModule.Application.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application.Events.Services;

public sealed class EventDispatcher(IServiceProvider services) : IEventDispatcher
{
    private const string HandleMethodName = "HandleAsync";

    public async Task DispatchBeforeCommitAsync(
        IReadOnlyList<EventContext> contexts,
        CancellationToken cancellationToken)
    {
        await DispatchAsync(typeof(IBeforeCommitEventHandler<>), contexts, cancellationToken);
    }

    public async Task DispatchAfterCommitAsync(
        IReadOnlyList<EventContext> contexts,
        CancellationToken cancellationToken)
    {
        await DispatchAsync(typeof(IAfterCommitEventHandler<>), contexts, cancellationToken);
    }

    private async Task DispatchAsync(
        Type contract,
        IReadOnlyList<EventContext> contexts,
        CancellationToken cancellationToken)
    {
        foreach (var context in contexts)
        {
            var handlerType = contract.MakeGenericType(context.Event.GetType());
            var handlers = services.GetServices(handlerType);
            var method = handlerType.GetMethod(HandleMethodName)!;

            foreach (var handler in handlers)
            {
                object[] arguments = [context.Event, context, cancellationToken];
                var running = (Task)method.Invoke(handler, BindingFlags.DoNotWrapExceptions, null, arguments, null)!;

                await running;
            }
        }
    }
}
