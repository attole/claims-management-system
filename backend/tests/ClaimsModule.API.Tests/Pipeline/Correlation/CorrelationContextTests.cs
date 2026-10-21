using ClaimsModule.API.Pipeline.Correlation;

namespace ClaimsModule.API.Tests.Pipeline.Correlation;

public sealed class CorrelationContextTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Correlation")]
    public void CorrelationId_ForANewContext_IsAFreshNonEmptyGuid()
    {
        var context = new CorrelationContext();

        context.CorrelationId.Should().NotBeEmpty();
    }

    [Fact]
    public void CorrelationId_ForTwoContexts_Differs()
    {
        var first = new CorrelationContext();
        var second = new CorrelationContext();

        first.CorrelationId.Should().NotBe(second.CorrelationId);
    }

    [Fact]
    public void CorrelationId_ReadTwice_StaysTheSame()
    {
        var context = new CorrelationContext();
        var first = context.CorrelationId;

        context.CorrelationId.Should().Be(first);
    }
}
