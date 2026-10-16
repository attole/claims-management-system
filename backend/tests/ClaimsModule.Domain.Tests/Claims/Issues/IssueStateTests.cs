using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Tests.Claims.Issues;

public sealed class IssueStateTests
{
    private static readonly IssueState[] _states =
    [
        new IssueRaisedState(),
        new IssueClearedState(),
        new IssueAcknowledgedState(),
        new IssueWaivedState(),
    ];

    [Fact]
    public void IssueStates_OnlyTheRaisedOneIsUnresolved()
    {
        _states.Where(state => !state.IsResolved).Should().ContainSingle().Which.Should().Be(new IssueRaisedState());
    }

    [Fact]
    public void IssueStates_CoverEveryKind()
    {
        _states.Select(state => state.Marker).Should().BeEquivalentTo(Enum.GetValues<IssueStateKind>());
    }

    [Fact]
    public void IssueStates_OnlyTheClearedOneIsNotCurrent()
    {
        _states.Where(state => !state.IsCurrent).Should().ContainSingle().Which.Should().Be(new IssueClearedState());
    }

    [Fact]
    public void IssueStates_OnlyTheWaivedOneIsWaived()
    {
        _states.Where(state => state.IsWaived).Should().ContainSingle().Which.Should().Be(new IssueWaivedState());
    }
}
