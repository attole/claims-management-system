using ClaimsModule.Application.Claims.Commands;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record AssignHandlerRequest
{
    public Guid HandlerId { get; init; } = Guid.Empty;

    public AssignHandlerCommand ToCommand(Guid claimId)
    {
        return new AssignHandlerCommand(claimId, HandlerId);
    }
}
