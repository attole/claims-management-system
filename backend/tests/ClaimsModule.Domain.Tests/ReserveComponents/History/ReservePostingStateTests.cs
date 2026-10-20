using ClaimsModule.Domain.ReserveComponents.History;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History;

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

    [Fact]
    public void IsPosted_IsTrueOnlyForThePostedState()
    {
        var result = _pairs.Where(pair => pair.State.IsPosted).Select(pair => pair.Kind);

        result.Should().Equal(ReservePostingStateKind.Posted);
    }
}
