using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.Tests.Core.Transitions;

public sealed class TransitionFindingTests
{
    [Fact]
    public void Create_NamesTheSourceAndTheTarget()
    {
        var result = TransitionFinding.Create("Draft", "Closed");

        result.Code.Should().Be(TransitionFinding.DefaultCode);
        result.From.Should().Be("Draft");
        result.To.Should().Be("Closed");
        result.Message.Should().Contain("Draft").And.Contain("Closed");
    }

    [Fact]
    public void Create_LeavesTheValidTargetsForTheApplicationLayer()
    {
        var result = TransitionFinding.Create("Draft", "Closed");

        result.ValidTargets.Should().BeEmpty();
    }
}
