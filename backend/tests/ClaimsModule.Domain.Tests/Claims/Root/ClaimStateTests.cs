using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Tests.Claims.Root;

public sealed class ClaimStateTests
{
    private static readonly (ClaimState State, ClaimStatus Status)[] _pairs =
    [
        (new DraftState(), ClaimStatus.Draft),
        (new OpenState(), ClaimStatus.Open),
        (new UnderInvestigationState(), ClaimStatus.UnderInvestigation),
        (new PendingPaymentState(), ClaimStatus.PendingPayment),
        (new ClosedState(), ClaimStatus.Closed),
        (new ReopenedState(), ClaimStatus.Reopened),
        (new WithdrawnState(), ClaimStatus.Withdrawn),
    ];

    [Fact]
    public void EveryStatusHasOneState()
    {
        _pairs.Select(pair => pair.Status).Should().BeEquivalentTo(Enum.GetValues<ClaimStatus>());
    }

    [Fact]
    public void EveryStateCarriesItsOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(
            pair => pair.State.Marker == pair.Status && pair.State.Name == pair.Status.ToString());
    }
}
