using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

public sealed record RetractHistoryInput(Guid ReserveHistoryId, Guid ActorId, DateTimeOffset Now)
{
    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);

    public string Reason { get; init; } = string.Empty;
}
