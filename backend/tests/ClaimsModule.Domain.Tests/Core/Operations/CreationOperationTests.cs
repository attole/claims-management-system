using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Operations;

public sealed class CreationOperationTests
{
    [Fact]
    public void Execute_ReturnsTheCreatedValueWithoutASubject()
    {
        var input = new SampleInput("created");

        var outcome = SampleCreationOperation.Execute(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("created");
    }
}
