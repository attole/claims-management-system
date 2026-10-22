namespace ClaimsModule.Application.Audit.Contracts;

public static class AuditEventTypes
{
    public const string ClaimCreated = "CLAIM_CREATED";

    public const string StatusChanged = "STATUS_CHANGED";

    public const string ClaimClosed = "CLAIM_CLOSED";

    public const string ClaimReopened = "CLAIM_REOPENED";

    public const string PartyAdded = "PARTY_ADDED";

    public const string PartyRemoved = "PARTY_REMOVED";

    public const string ReserveCreated = "RESERVE_CREATED";

    public const string ReserveAutoApproved = "RESERVE_AUTO_APPROVED";

    public const string ReserveApproved = "RESERVE_APPROVED";

    public const string ReserveRejected = "RESERVE_REJECTED";

    public const string ReserveRetracted = "RESERVE_RETRACTED";

    public const string GlPostingSimulated = "GL_POSTING_SIMULATED";

    public const string GlPostingFailed = "GL_POSTING_FAILED";

    public const string DocumentUploaded = "DOCUMENT_UPLOADED";

    public const string ValidationIssueAdded = "VALIDATION_ISSUE_ADDED";

    public const string SlaBreachDetected = "SLA_BREACH_DETECTED";

    public static class Additional
    {
        public const string ValidationIssueAcknowledged = "VALIDATION_ISSUE_ACKNOWLEDGED";

        public const string ValidationIssueWaived = "VALIDATION_ISSUE_WAIVED";

        public const string LimitOverrideChanged = "LIMIT_OVERRIDE_CHANGED";

        public const string ReserveComponentStateChanged = "RESERVE_COMPONENT_STATE_CHANGED";

        public const string DocumentRemoved = "DOCUMENT_REMOVED";

        public const string HandlerChanged = "HANDLER_CHANGED";
    }
}
