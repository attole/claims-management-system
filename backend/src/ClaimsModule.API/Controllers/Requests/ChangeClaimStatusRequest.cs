using ClaimsModule.Application.Claims.StateChanges;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record ChangeClaimStatusRequest(ClaimStateKind TargetStatus)
{
    public string ReasonCode { get; init; } = string.Empty;

    public string ReasonText { get; init; } = string.Empty;

    public string Note { get; init; } = string.Empty;

    public bool IsLiabilityConfirmed { get; init; } = false;

    public ChangeClaimStatusCommand ToCommand(Guid claimId)
    {
        return new ChangeClaimStatusCommand(claimId, TargetStatus)
        {
            ReasonCode = ReasonCode,
            ReasonText = ReasonText,
            Note = Note,
            IsLiabilityConfirmed = IsLiabilityConfirmed,
        };
    }
}
