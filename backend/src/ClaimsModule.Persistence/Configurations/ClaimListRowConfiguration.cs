using ClaimsModule.Persistence.Rows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimListRowConfiguration(ClaimsDbContext context)
    : TenantConfiguration<ClaimListRow>(context)
{
    private const string ListSql = """
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

    public override void Configure(EntityTypeBuilder<ClaimListRow> builder)
    {
        base.Configure(builder);

        builder.HasNoKey();
        builder.ToSqlQuery(ListSql);
        builder.Property(row => row.LossDate).IsRequired(false);
    }
}
