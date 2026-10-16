using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ReserveChanges.Inputs;

public record ReserveDecisionInput(Guid ActorId, DateTimeOffset Now)
{
    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);

    public string Reason { get; init; } = string.Empty;

    public virtual Guid ReviewerId => ActorId;
}
