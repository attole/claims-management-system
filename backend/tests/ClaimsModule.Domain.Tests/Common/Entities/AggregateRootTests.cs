using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Common.Entities;

public sealed class AggregateRootTests
{
    [Fact]
    public void RowVer_StartsEmpty()
    {
        new SampleRoot(Guid.NewGuid()).RowVer.Should().BeEmpty();
    }
}
