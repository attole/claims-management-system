using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Transitions;

public sealed class AllowedSourcesBuilderTests
{
    [Fact]
    public void AllowedFrom_WithADuplicateSource_Throws()
    {
        Func<DuplicateSourceSampleTransition> act = () => new DuplicateSourceSampleTransition();

        act.Should().Throw<InvalidOperationException>().WithMessage("*Open*");
    }

    [Fact]
    public void AllowedFrom_WithoutSources_Throws()
    {
        Func<NoSourcesSampleTransition> act = () => new NoSourcesSampleTransition();

        act.Should().Throw<ArgumentException>();
    }
}
