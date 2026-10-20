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

    public static Error StatusUnsupported(string targetStatus, IReadOnlyList<string> nextStatuses)
    {
        string message = $"A claim cannot be moved to {targetStatus}. {NextStatusesText(nextStatuses)}";

        return Error.Validation(StatusUnsupportedCode, message, "TargetStatus");
    }

    public static Error WithNextStatuses(Error refusal, IReadOnlyList<string> nextStatuses)
    {
        string message = $"{refusal.Message} {NextStatusesText(nextStatuses)}";

        return Error.Validation(refusal.Code, message, "TargetStatus");
    }

    public static Error PolicyNotFound(Guid policyId)
    {
        return Error.NotFound(PolicyNotFoundCode, $"Policy {policyId} does not exist.");
    }

    public static Warning PolicyNotFoundWarning(Guid policyId)
    {
        string message = $"Policy {policyId} does not exist; the claim has no policy linked.";

        return new Warning(PolicyNotFoundCode, message, "PolicyId");
    }

    private static string NextStatusesText(IReadOnlyList<string> nextStatuses)
    {
        string next = nextStatuses.Count == 0 ? "none" : string.Join(", ", nextStatuses);

        return $"Valid next statuses: {next}.";
    }
}

public static class ClaimFieldLengths
{
    public const int CauseOfLossCode = 50;
    public const int Location = 500;
    public const int PoliceReportNumber = 100;
    public const int FirstName = 100;
    public const int LastName = 100;
    public const int CompanyName = 255;
    public const int Email = 255;
    public const int Phone = 50;
    public const int AssetDescription = 500;
    public const int AssetReference = 255;
    public const int SearchText = 255;
    public const int ReasonCode = 50;
    public const int ReasonText = 500;
}
