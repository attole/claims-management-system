using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Flows;

public sealed class CreationOperationTests
{
    [Fact]
    public void Run_ReturnsTheCreatedValueWithoutASubject()
    {
        var input = new SampleInput("created");

        var outcome = SampleCreationOperation.Run(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("created");
    }
}
