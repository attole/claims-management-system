namespace ClaimsModule.Domain.ReserveChanges.Root.Inputs;

public sealed record MarkReservePostingFailedInput(Guid ActorId, DateTimeOffset Now)
    : ReserveChangeBasePostingTransitionInput(ActorId, Now);
