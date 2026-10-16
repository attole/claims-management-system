using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record AcknowledgeIssueInput(Guid IssueId, Guid ActorId, DateTimeOffset Now, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference)
{
    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
