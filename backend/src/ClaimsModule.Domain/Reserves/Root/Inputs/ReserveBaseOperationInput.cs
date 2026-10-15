using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Reserves.Root.Inputs;

public abstract record ReserveBaseOperationInput(DateTimeOffset Now, Guid ActorId) : OperationInput(Now, ActorId);
