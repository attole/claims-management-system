using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Claims.Issues;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.ReserveComponents.Dtos;

namespace ClaimsModule.Application.Claims.Dtos;

public sealed record ClaimDetailDto
{
    public ClaimDto Claim { get; init; } = new();

    public string ClientName { get; init; } = string.Empty;

    public string PolicyNumber { get; init; } = string.Empty;

    public LossDetailsDto Loss { get; init; } = new();

    public IReadOnlyList<PartyDto> Parties { get; init; } = [];

    public IReadOnlyList<RiskObjectDto> RiskObjects { get; init; } = [];

    public IReadOnlyList<IssueDto> Issues { get; init; } = [];

    public ClaimReservesDto Reserves { get; init; } = new();

    public IReadOnlyList<ClaimDocumentLinkDto> Documents { get; init; } = [];

    public IReadOnlyList<AuditEntryDto> RecentAudit { get; init; } = [];
}
