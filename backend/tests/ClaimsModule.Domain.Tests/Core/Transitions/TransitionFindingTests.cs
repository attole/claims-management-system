using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.Tests.Core.Transitions;

public sealed class TransitionFindingTests
{
    [Fact]
    public void Constructor_NamesTheSourceAndTheTarget()
    {
        var result = new TransitionFinding("Draft", "Closed");

        result.Code.Should().Be(TransitionFinding.DefaultCode);
        result.From.Should().Be("Draft");
        result.To.Should().Be("Closed");
        result.Message.Should().Contain("Draft").And.Contain("Closed");
    }

    [Fact]
    public void Constructor_LeavesTheValidTargetsForTheApplicationLayer()
    {
        var result = new TransitionFinding("Draft", "Closed");

        result.ValidTargets.Should().BeEmpty();
    }
}
