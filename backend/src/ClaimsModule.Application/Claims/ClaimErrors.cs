using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Claims;

public static class ClaimErrors
{
    public const string ClaimNotFoundCode = "CLAIM_NOT_FOUND";
    public const string PartyNotFoundCode = "PARTY_NOT_FOUND";
    public const string RiskObjectNotFoundCode = "RISK_OBJECT_NOT_FOUND";
    public const string IssueNotFoundCode = "ISSUE_NOT_FOUND";
    public const string PolicyNotFoundCode = "POLICY_NOT_FOUND";
    public const string StatusUnsupportedCode = "STATUS_TARGET_UNSUPPORTED";

    public static Error ClaimNotFound(Guid claimId)
    {
        return Error.NotFound(ClaimNotFoundCode, $"Claim {claimId} does not exist.");
    }

    public static Error PartyNotFound(Guid partyId)
    {
        return Error.NotFound(PartyNotFoundCode, $"Party {partyId} does not exist on the claim.");
    }

    public static Error RiskObjectNotFound(Guid riskObjectId)
    {
        return Error.NotFound(RiskObjectNotFoundCode, $"Risk object {riskObjectId} does not exist on the claim.");
    }

    public static Error IssueNotFound(Guid issueId)
    {
        return Error.NotFound(IssueNotFoundCode, $"Issue {issueId} does not exist on the claim.");
    }

    public static Error StatusUnsupported(string targetStatus)
    {
        return Error.Validation(StatusUnsupportedCode, $"A claim cannot be moved to {targetStatus}.", "TargetStatus");
    }

    public static Error PolicyNotFound(Guid policyId)
    {
        return Error.NotFound(PolicyNotFoundCode, $"Policy {policyId} does not exist.");
    }
}
