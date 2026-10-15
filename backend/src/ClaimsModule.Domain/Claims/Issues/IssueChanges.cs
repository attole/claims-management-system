namespace ClaimsModule.Domain.Claims.Issues;

public sealed record IssueChanges(
    IReadOnlyList<ClaimValidationIssue> Raised,
    IReadOnlyList<ClaimValidationIssue> Cleared);
