using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Persistence.Conventions;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClaimsModule.Persistence.Tests.Conventions;

public sealed class EntityTableConventionTests
{
    [Theory]
    [Trait("Rule", "S:DATA-AuditColumns")]
    [InlineData("CreatedAt")]
    [InlineData("UpdatedAt")]
    [InlineData("UserCreated")]
    [InlineData("UserModified")]
    public void ConfigureEntityTables_EntityTable_AddsTheAuditColumns(string column)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CauseOfLossCode))!;

        var result = entityType.FindProperty(column);

        result.Should().NotBeNull();
    }

    [Theory]
    [Trait("Rule", "S:DATA-SoftDelete")]
    [InlineData("IsDeleted")]
    [InlineData("DeletedAt")]
    public void ConfigureEntityTables_EntityTable_AddsTheSoftDeleteColumns(string column)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CauseOfLossCode))!;

        var result = entityType.FindProperty(column);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Rule", "S:DATA-EfConfig")]
    public void ConfigureEntityTables_EntityTable_KeepsTheKeyClientGenerated()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CauseOfLossCode))!;

        var result = entityType.FindProperty("Id")!.ValueGenerated;

        result.Should().Be(ValueGenerated.Never);
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void HasSoftDelete_EntityTable_IsTrue()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CauseOfLossCode))!;

        var result = EntityTableConvention.HasSoftDelete(entityType);

        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void HasSoftDelete_RowWithoutTheSoftDeleteInterface_IsFalse()
    {
        using var database = new TestDatabase();
        using var context = database.CreateConventionContext();
        var entityType = context.Model.FindEntityType(typeof(AppendOnlyRow))!;

        var result = EntityTableConvention.HasSoftDelete(entityType);

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:DATA-EfConfig")]
    public void ConfigureEntityTables_SqlServerKey_DefaultsToASequentialId()
    {
        var model = SqlServerModel.Create();
        var key = model.FindEntityType(typeof(CauseOfLossCode))!.FindProperty("Id")!;

        var result = key.GetDefaultValueSql();

        result.Should().Be("NEWSEQUENTIALID()");
    }

    [Fact]
    [Trait("Rule", "S:DATA-Conventions")]
    public void ConfigureEntityTables_SqlServerTimestamps_AreStoredAsDateTimeOffset()
    {
        var model = SqlServerModel.Create();
        var createdAt = model.FindEntityType(typeof(CauseOfLossCode))!.FindProperty("CreatedAt")!;

        var result = createdAt.GetColumnType();

        result.Should().Be("datetimeoffset");
    }
}
