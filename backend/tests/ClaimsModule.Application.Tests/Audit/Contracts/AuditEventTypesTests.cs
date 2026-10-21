using ClaimsModule.Application.Audit.Contracts;

namespace ClaimsModule.Application.Tests.Audit.Contracts;

public sealed class AuditEventTypesTests
{
    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    [Trait("Rule", "S:AUD-Events")]
    public void Constants_AreTheFixedEventTypesOfTheSpec()
    {
        var result = typeof(AuditEventTypes)
            .GetFields()
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();

        result.Should().BeEquivalentTo(
            "CLAIM_CREATED",
            "STATUS_CHANGED",
            "PARTY_ADDED",
            "PARTY_REMOVED",
            "RESERVE_CREATED",
            "RESERVE_AUTO_APPROVED",
            "RESERVE_APPROVED",
            "RESERVE_REJECTED",
            "RESERVE_RETRACTED",
            "GL_POSTING_SIMULATED",
            "GL_POSTING_FAILED",
            "DOCUMENT_UPLOADED",
            "CLAIM_CLOSED",
            "CLAIM_REOPENED",
            "SLA_BREACH_DETECTED",
            "VALIDATION_ISSUE_ADDED");
    }

    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public void AdditionalConstants_AreTheEventTypesBeyondTheSpecList()
    {
        var result = typeof(AuditEventTypes.Additional)
            .GetFields()
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();

        result.Should().BeEquivalentTo(
            "VALIDATION_ISSUE_ACKNOWLEDGED",
            "VALIDATION_ISSUE_WAIVED",
            "LIMIT_OVERRIDE_CHANGED",
            "RESERVE_COMPONENT_STATE_CHANGED",
            "DOCUMENT_REMOVED");
    }
}
