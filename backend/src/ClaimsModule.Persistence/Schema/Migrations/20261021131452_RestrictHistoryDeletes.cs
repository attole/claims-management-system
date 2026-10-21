using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsModule.Persistence.Schema.Migrations
{
    /// <inheritdoc />
    public partial class RestrictHistoryDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimAuditLog_Claims_ClaimId",
                table: "ClaimAuditLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ClaimStateChanges_Claims_ClaimId",
                table: "ClaimStateChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_ReserveHistory_ClaimReserveComponents_ReserveComponentId",
                table: "ReserveHistory");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimAuditLog_Claims_ClaimId",
                table: "ClaimAuditLog",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimStateChanges_Claims_ClaimId",
                table: "ClaimStateChanges",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReserveHistory_ClaimReserveComponents_ReserveComponentId",
                table: "ReserveHistory",
                column: "ReserveComponentId",
                principalTable: "ClaimReserveComponents",
                principalColumn: "ReserveComponentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimAuditLog_Claims_ClaimId",
                table: "ClaimAuditLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ClaimStateChanges_Claims_ClaimId",
                table: "ClaimStateChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_ReserveHistory_ClaimReserveComponents_ReserveComponentId",
                table: "ReserveHistory");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimAuditLog_Claims_ClaimId",
                table: "ClaimAuditLog",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimStateChanges_Claims_ClaimId",
                table: "ClaimStateChanges",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReserveHistory_ClaimReserveComponents_ReserveComponentId",
                table: "ReserveHistory",
                column: "ReserveComponentId",
                principalTable: "ClaimReserveComponents",
                principalColumn: "ReserveComponentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
