using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Core.Operations;

public abstract record OperationInput(DateTimeOffset Now, Guid ActorId)
{
    public DateTimeOffset Now { get; } = Now != default
        ? Now
        : throw new ArgumentException("An operation input needs the time of the operation.", nameof(Now));

    public EventStamp Stamp()
    {
        return new EventStamp(Ids.Next(Now), Now, ActorId);
    }
}
