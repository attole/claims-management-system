using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Domain.Tests.Claims;

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
    [Trait("Rule", "S:STATUS-Values")]
    [Trait("Rule", "A:STATUS-Values")]
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

    [Fact]
    public void IsEditable_IsFalseOnlyForClosedAndWithdrawn()
    {
        var frozen = _pairs.Where(pair => !pair.State.IsEditable).Select(pair => pair.Kind);

        frozen.Should().BeEquivalentTo([ClaimStateKind.Closed, ClaimStateKind.Withdrawn]);
    }

    [Fact]
    public void IsDraft_IsTrueOnlyForDraft()
    {
        var drafts = _pairs.Where(pair => pair.State.IsDraft).Select(pair => pair.Kind);

        drafts.Should().BeEquivalentTo([ClaimStateKind.Draft]);
    }
}
