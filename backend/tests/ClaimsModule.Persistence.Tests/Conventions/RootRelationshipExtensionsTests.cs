using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Persistence.Conventions;
using ClaimsModule.Persistence.Rows;
using ClaimsModule.Persistence.Tests.Support;

namespace ClaimsModule.Persistence.Tests.Conventions;

public sealed class RootRelationshipExtensionsTests
{
    [Fact]
    [Trait("Rule", "S:DATA-RowVer")]
    public void IsMarkedToModifyRoot_MarkedRelationship_IsTrue()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var key = context.Model.FindEntityType(typeof(ReserveHistory))!.GetForeignKeys().Single();

        var result = key.IsMarkedToModifyRoot();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsMarkedToModifyRoot_UnmarkedRelationship_IsFalse()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var key = context.Model.FindEntityType(typeof(PolicyCoverageType))!.GetForeignKeys().Single();

        var result = key.IsMarkedToModifyRoot();

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:DATA-RowVer")]
    public void Model_RelationshipToAnAggregateRoot_IsMarkedToModifyIt()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetForeignKeys())
            .Where(key => typeof(AggregateRoot).IsAssignableFrom(key.PrincipalEntityType.ClrType))
            .Where(key => !key.IsMarkedToModifyRoot())
            .ToList();

        result.Should().BeEmpty();
    }
}
