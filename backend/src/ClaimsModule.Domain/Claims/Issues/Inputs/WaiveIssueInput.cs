using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record WaiveIssueInput(
    Guid IssueId,
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    LossReference LossReference) : ClaimBaseOperationInput(LossReference)
{
    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
