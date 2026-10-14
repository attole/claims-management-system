using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Tests.Claims.Root;

public sealed class ClaimStateTests
{
    private static readonly (ClaimState State, ClaimStateKind Kind)[] _pairs =
    [
        (new ClaimDraftState(), ClaimStateKind.Draft),
        (new ClaimOpenState(), ClaimStateKind.Open),
        (new ClaimUnderInvestigationState(), ClaimStateKind.UnderInvestigation),
        (new ClaimPendingPaymentState(), ClaimStateKind.PendingPayment),
        (new ClaimClosedState(), ClaimStateKind.Closed),
        (new ClaimReopenedState(), ClaimStateKind.Reopened),
        (new ClaimWithdrawnState(), ClaimStateKind.Withdrawn),
    ];

    [Fact]
    public void ClaimStates_CoverEveryKind()
    {
        _pairs.Select(pair => pair.Kind).Should().BeEquivalentTo(Enum.GetValues<ClaimStateKind>());
    }

    [Fact]
    public void ClaimStates_CarryTheirOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(
            pair => pair.State.Marker == pair.Kind && pair.State.Name == pair.Kind.ToString());
    }
}
