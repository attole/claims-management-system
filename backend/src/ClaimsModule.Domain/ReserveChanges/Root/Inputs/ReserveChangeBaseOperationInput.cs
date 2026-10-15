using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.ReserveChanges.Root.Inputs;

public abstract record ReserveChangeBaseOperationInput(DateTimeOffset Now, Guid ActorId) : OperationInput(Now, ActorId);
