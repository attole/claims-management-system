using ClaimsModule.Application.Claims.Commands;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record LinkPolicyRequest
{
    public Guid PolicyId { get; init; } = Guid.Empty;

    public LinkPolicyCommand ToCommand(Guid claimId)
    {
        return new LinkPolicyCommand(claimId, PolicyId);
    }
}
