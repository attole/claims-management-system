namespace ClaimsModule.Domain.ReserveChanges.Root.Inputs;

public sealed record MarkReservePostedInput(Guid ActorId, DateTimeOffset Now)
    : ReserveChangeBasePostingTransitionInput(ActorId, Now);
