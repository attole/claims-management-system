namespace ClaimsModule.Persistence.Rows;

internal sealed class ClaimListRow
{
    public Guid ClaimId { get; private set; } = Guid.Empty;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    public string ClaimNumber { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public Guid PolicyId { get; private set; } = Guid.Empty;

    public Guid AssignedHandlerId { get; private set; } = Guid.Empty;

    public DateTimeOffset? LossDate { get; private set; }

    public string CauseOfLossCode { get; private set; } = string.Empty;

    public string PolicyNumber { get; private set; } = string.Empty;

    public string ClientName { get; private set; } = string.Empty;
}
