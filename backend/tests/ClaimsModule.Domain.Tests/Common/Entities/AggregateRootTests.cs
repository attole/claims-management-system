using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Common.Entities;

public sealed class AggregateRootTests
{
    [Fact]
    public void RowVer_StartsEmpty()
    {
        var root = new SampleRoot(Guid.NewGuid());

        root.RowVer.Should().BeEmpty();
    }
}
