using ClaimsModule.Domain.ReserveChanges.Root;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root;

public sealed class ReservePostingStateTests
{
    private static readonly (ReservePostingState State, ReservePostingStateKind Kind)[] _pairs =
    [
        (new ReservePostingPendingState(), ReservePostingStateKind.Pending),
        (new ReservePostingPostedState(), ReservePostingStateKind.Posted),
        (new ReservePostingFailedState(), ReservePostingStateKind.Failed),
        (new ReservePostingCancelledState(), ReservePostingStateKind.Cancelled),
    ];

    [Fact]
    public void Postings_CoverEveryKind()
    {
        _pairs.Select(pair => pair.Kind).Should().BeEquivalentTo(Enum.GetValues<ReservePostingStateKind>());
    }

    [Fact]
    public void Postings_CarryTheirOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(
            pair => pair.State.Marker == pair.Kind && pair.State.Name == pair.Kind.ToString());
    }
}
