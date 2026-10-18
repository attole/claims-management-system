using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

public sealed record RejectHistoryInput(Guid ReserveHistoryId, UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
{
    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);

    public string Reason { get; init; } = string.Empty;
}
