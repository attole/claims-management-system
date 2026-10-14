using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Operations.Creation;

public sealed class CreationOperationTests
{
    [Fact]
    public void Execute_ReturnsTheCreatedValueWithoutASubject()
    {
        var operation = new SampleCreationOperation();
        var input = new SampleInput("created");

        var outcome = operation.Execute(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("created");
    }

    [Fact]
    public void Execute_RunsAfterCreateWithTheCreatedValueAfterApply()
    {
        var operation = new SampleCreationOperation();
        var input = new SampleInput("created");

        operation.Execute(input);

        operation.Log.Should().Equal("apply created", "after created");
    }
}
