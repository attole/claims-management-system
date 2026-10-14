using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Operations;

public sealed class OperationInputTests
{
    [Fact]
    public void Input_KeepsTheTimeItWasGiven()
    {
        var input = new SampleInput("text", SampleInput.Instant);

        input.Now.Should().Be(SampleInput.Instant);
    }

    [Fact]
    public void Input_WithoutATime_Throws()
    {
        Func<SampleInput> act = () => new SampleInput("text", default);

        act.Should().Throw<ArgumentException>();
    }
}
