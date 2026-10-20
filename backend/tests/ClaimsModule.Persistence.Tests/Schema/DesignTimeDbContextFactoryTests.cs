using ClaimsModule.Persistence.Schema;

namespace ClaimsModule.Persistence.Tests.Schema;

public sealed class DesignTimeDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_NoArguments_BuildsAContextWithTheModel()
    {
        var factory = new DesignTimeDbContextFactory();

        using var result = factory.CreateDbContext([]);

        result.Model.GetEntityTypes().Should().NotBeEmpty();
    }
}
