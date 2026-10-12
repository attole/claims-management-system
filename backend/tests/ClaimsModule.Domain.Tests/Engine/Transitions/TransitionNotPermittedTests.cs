using ClaimsModule.Domain.Engine.Transitions;

namespace ClaimsModule.Domain.Tests.Engine.Transitions;

public sealed class TransitionNotPermittedTests
{
    [Fact]
    public void Create_NamesTheSourceAndTheTarget()
    {
        TransitionNotPermitted error = TransitionNotPermitted.Create("CODE", "Draft", "Closed");

        error.Code.Should().Be("CODE");
        error.From.Should().Be("Draft");
        error.To.Should().Be("Closed");
        error.Message.Should().Contain("Draft").And.Contain("Closed");
    }

    [Fact]
    public void Create_LeavesTheValidTargetsForTheApplicationLayer()
    {
        TransitionNotPermitted.Create("CODE", "Draft", "Closed").ValidTargets.Should().BeEmpty();
    }
}
