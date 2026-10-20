using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsModule.Persistence.Schema.Migrations
{
    /// <inheritdoc />
    public partial class ClaimListViewAndAuditGrants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE VIEW dbo.ClaimListView AS
                SELECT claim.ClaimId, claim.OrganisationId, claim.ClaimNumber, claim.Status, claim.PolicyId,
                    claim.AssignedHandlerId, loss.LossDate, loss.CauseOfLossCode,
                    COALESCE(policy.PolicyNumber, '') AS PolicyNumber, COALESCE(policy.ClientName, '') AS ClientName
                FROM Claims AS claim
                INNER JOIN LossEvents AS loss ON loss.ClaimId = claim.ClaimId AND loss.IsDeleted = 0
                    AND loss.OrganisationId = claim.OrganisationId
                LEFT JOIN Policies AS policy ON policy.PolicyId = claim.PolicyId AND policy.IsDeleted = 0
                    AND policy.OrganisationId = claim.OrganisationId
                WHERE claim.IsDeleted = 0
                """);

            migrationBuilder.Sql("""
                IF DATABASE_PRINCIPAL_ID('claims_app') IS NULL
                    CREATE ROLE claims_app
                """);

            migrationBuilder.Sql("DENY UPDATE, DELETE ON dbo.ClaimAuditLog TO claims_app");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE UPDATE, DELETE ON dbo.ClaimAuditLog FROM claims_app");

            migrationBuilder.Sql("DROP VIEW dbo.ClaimListView");
        }
    }
}
