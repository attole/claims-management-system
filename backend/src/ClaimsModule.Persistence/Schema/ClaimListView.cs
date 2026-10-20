namespace ClaimsModule.Persistence.Schema;

internal static class ClaimListView
{
    public const string Name = "ClaimListView";

    public const string SelectSql = """
        SELECT claim.ClaimId, claim.OrganisationId, claim.ClaimNumber, claim.Status, claim.PolicyId,
            claim.AssignedHandlerId, loss.LossDate, loss.CauseOfLossCode,
            COALESCE(policy.PolicyNumber, '') AS PolicyNumber, COALESCE(policy.ClientName, '') AS ClientName
        FROM Claims AS claim
        INNER JOIN LossEvents AS loss ON loss.ClaimId = claim.ClaimId AND loss.IsDeleted = 0
            AND loss.OrganisationId = claim.OrganisationId
        LEFT JOIN Policies AS policy ON policy.PolicyId = claim.PolicyId AND policy.IsDeleted = 0
            AND policy.OrganisationId = claim.OrganisationId
        WHERE claim.IsDeleted = 0
        """;
}
