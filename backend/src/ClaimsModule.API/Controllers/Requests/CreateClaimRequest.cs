using ClaimsModule.Application.Claims.Commands;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.ReserveComponents.Requests;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record CreateClaimRequest
{
    public Guid PolicyId { get; init; } = Guid.Empty;

    public DateTimeOffset ReportDate { get; init; } = DateTimeOffset.MinValue;

    public LossRequest Loss { get; init; } = new(DateTimeOffset.MinValue, string.Empty, string.Empty);

    public IReadOnlyList<PartyRequest> Parties { get; init; } = [];

    public IReadOnlyList<RiskObjectRequest> RiskObjects { get; init; } = [];

    public IReadOnlyList<ReserveRequest> Reserves { get; init; } = [];

    public CreateClaimCommand ToCommand()
    {
        return new CreateClaimCommand(PolicyId, ReportDate, Loss, Parties, RiskObjects) { Reserves = Reserves };
    }
}
