using ClaimsModule.Domain.Tests.Support.Engine;

namespace ClaimsModule.Domain.Tests.Engine.Operations;

public sealed class OperationInputTests
{
    [Fact]
    public void Input_KeepsTheTimeItWasGiven()
    {
        SampleInput input = new("text", SampleInput.Instant);

        input.Now.Should().Be(SampleInput.Instant);
    }

    [Fact]
    public void Input_WithoutATime_Throws()
    {
        Func<SampleInput> create = () => new SampleInput("text", default);

        create.Should().Throw<ArgumentException>();
    }
}
