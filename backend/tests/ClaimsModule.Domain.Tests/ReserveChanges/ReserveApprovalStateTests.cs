using ClaimsModule.Domain.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges;

public sealed class ReserveApprovalStateTests
{
    private static readonly (ReserveApprovalState State, ReserveApprovalStateKind Kind)[] _pairs =
    [
        (new ReserveAutoApprovedState(), ReserveApprovalStateKind.AutoApproved),
        (new ReservePendingApprovalState(), ReserveApprovalStateKind.PendingApproval),
        (new ReserveApprovedState(), ReserveApprovalStateKind.Approved),
        (new ReserveRejectedState(), ReserveApprovalStateKind.Rejected),
        (new ReserveCancelledState(), ReserveApprovalStateKind.Cancelled),
    ];

    [Fact]
    public void Approvals_CoverEveryKind()
    {
        _pairs.Select(pair => pair.Kind).Should().BeEquivalentTo(Enum.GetValues<ReserveApprovalStateKind>());
    }

    [Fact]
    public void Approvals_CarryTheirOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(
            pair => pair.State.Marker == pair.Kind && pair.State.Name == pair.Kind.ToString());
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_IsTrueOnlyForAutoApprovedAndApproved()
    {
        var approved = _pairs.Where(pair => pair.State.IsApproved).Select(pair => pair.Kind);

        approved.Should().BeEquivalentTo(
            [ReserveApprovalStateKind.AutoApproved, ReserveApprovalStateKind.Approved]);
    }

    [Fact]
    public void IsRefused_IsTrueOnlyForRejectedAndCancelled()
    {
        var refused = _pairs.Where(pair => pair.State.IsRefused).Select(pair => pair.Kind);

        refused.Should().BeEquivalentTo([ReserveApprovalStateKind.Rejected, ReserveApprovalStateKind.Cancelled]);
    }
}
