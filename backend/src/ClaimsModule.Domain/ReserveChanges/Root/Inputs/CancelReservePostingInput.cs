namespace ClaimsModule.Domain.ReserveChanges.Root.Inputs;

public sealed record CancelReservePostingInput(Guid ActorId, DateTimeOffset Now)
    : ReserveChangeBasePostingTransitionInput(ActorId, Now);
