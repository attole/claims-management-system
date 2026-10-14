using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Tests.Claims.Root;

public sealed class ClaimStateTests
{
    private static readonly (ClaimState State, ClaimStatus Status)[] _pairs =
    [
        (new ClaimDraftState(), ClaimStatus.Draft),
        (new ClaimOpenState(), ClaimStatus.Open),
        (new ClaimUnderInvestigationState(), ClaimStatus.UnderInvestigation),
        (new ClaimPendingPaymentState(), ClaimStatus.PendingPayment),
        (new ClaimClosedState(), ClaimStatus.Closed),
        (new ClaimReopenedState(), ClaimStatus.Reopened),
        (new ClaimWithdrawnState(), ClaimStatus.Withdrawn),
    ];

    [Fact]
    public void ClaimStates_CoverEveryStatus()
    {
        _pairs.Select(pair => pair.Status).Should().BeEquivalentTo(Enum.GetValues<ClaimStatus>());
    }

    [Fact]
    public void ClaimStates_CarryTheirOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(
            pair => pair.State.Marker == pair.Status && pair.State.Name == pair.Status.ToString());
    }
}
