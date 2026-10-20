using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record WaiveIssueInput(
    Guid IssueId,
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes) : ClaimBaseOperationInput(PolicyPeriod, CauseCodes, Now)
{
    public Guid ActorId { get; } = InputActor.Required(ActorId);
}
