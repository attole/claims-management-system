using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Persistence.Rows;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.CauseOfLoss;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClaimsModule.Persistence.Tests;

public sealed class ClaimsDbContextTests
{
    private static readonly string[] _tableNames =
    [
        "Claims",
        "LossEvents",
        "ClaimParties",
        "ClaimRiskObjects",
        "ClaimReserveComponents",
        "ReserveHistory",
        "ClaimDocuments",
        "ClaimAuditLog",
        "CauseOfLossCodes",
        "Policies",
        "ClaimIssues",
        "ClaimStateChanges",
        "PolicyCoverageTypes",
        "ClaimNumberCounters",
        "IdempotencyKeys",
    ];

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public void Query_RowOfAnotherOrganisation_IsHidden()
    {
        using var database = new TestDatabase();
        using (var writer = database.CreateContext())
        {
            writer.CauseOfLossCodes.Add(TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL"));
            writer.SaveChanges();
        }

        using var context = database.CreateContext(Ids.Next(), TestDatabase.UserId);

        var result = context.CauseOfLossCodes.ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public void Query_RowOfTheCurrentOrganisation_IsReturned()
    {
        using var database = new TestDatabase();
        using (var writer = database.CreateContext())
        {
            writer.CauseOfLossCodes.Add(TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL"));
            writer.SaveChanges();
        }

        using var context = database.CreateContext();

        var result = context.CauseOfLossCodes.ToList();

        result.Should().ContainSingle();
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void Query_RemovedRow_IsHidden()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var code = TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL");
        context.CauseOfLossCodes.Add(code);
        context.SaveChanges();
        context.CauseOfLossCodes.Remove(code);
        context.SaveChanges();

        var result = context.CauseOfLossCodes.ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public void Query_WithoutFilters_ReturnsEveryOrganisation()
    {
        using var database = new TestDatabase();
        using (var writer = database.CreateContext())
        {
            writer.CauseOfLossCodes.Add(TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL"));
            writer.SaveChanges();
        }

        using var context = database.CreateContext(Guid.Empty, Guid.Empty);

        var result = context.CauseOfLossCodes.IgnoreQueryFilters().ToList();

        result.Should().ContainSingle();
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public void Model_EveryTenantOwnedType_HasAQueryFilter()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Model.GetEntityTypes()
            .Where(entityType => typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType))
            .Where(entityType => entityType.GetQueryFilter() is null)
            .ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public void Model_ClaimListRow_HasAQueryFilter()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Model.FindEntityType(typeof(ClaimListRow))!.GetQueryFilter();

        result.Should().NotBeNull();
    }

    [Fact]
    public void Model_EveryConfigurationClass_IsApplied()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = typeof(ClaimsDbContext).Assembly.GetTypes()
            .Where(type => !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces())
            .Where(contract => contract.IsGenericType)
            .Where(contract => contract.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
            .Select(contract => contract.GetGenericArguments()[0])
            .Where(entityType => context.Model.FindEntityType(entityType) is null)
            .ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void Model_SoftDeletableEntityTable_FiltersTheDeleteFlag()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Model.GetEntityTypes()
            .Where(entityType => typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            .Where(entityType => !entityType.GetQueryFilter()!.ToString().Contains("IsDeleted"))
            .ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-Naming")]
    public void Model_Tables_AreNamedAfterTheSpecification()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Model.GetEntityTypes()
            .Where(entityType => entityType.GetSqlQuery() is null)
            .Select(entityType => entityType.GetTableName())
            .ToList();

        result.Should().OnlyContain(name => _tableNames.Contains(name));
    }

    [Fact]
    [Trait("Rule", "S:DATA-Naming")]
    public void Model_Columns_ArePascalCase()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetFlattenedProperties())
            .Select(property => property.GetColumnName())
            .ToList();

        result.Should().OnlyContain(name => char.IsUpper(name[0]) && !name.Contains('_'));
    }

    [Fact]
    [Trait("Rule", "S:DATA-Conventions")]
    public void Model_DecimalColumns_AreMoneyPrecision()
    {
        var model = SqlServerModel.Create();

        var result = model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetFlattenedProperties())
            .Where(property => property.ClrType == typeof(decimal))
            .ToList();

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(property => property.GetPrecision() == 19 && property.GetScale() == 4);
    }

    [Theory]
    [Trait("Rule", "S:DATA-AuditColumns")]
    [InlineData("CreatedAt")]
    [InlineData("UpdatedAt")]
    [InlineData("UserCreated")]
    [InlineData("UserModified")]
    public void Model_EntityTable_AddsTheAuditColumns(string column)
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
    public void Model_EntityTable_AddsTheSoftDeleteColumns(string column)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CauseOfLossCode))!;

        var result = entityType.FindProperty(column);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Rule", "S:DATA-EfConfig")]
    public void Model_EntityTable_KeepsTheKeyClientGenerated()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CauseOfLossCode))!;

        var result = entityType.FindProperty("Id")!.ValueGenerated;

        result.Should().Be(ValueGenerated.Never);
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void Model_EntityWithoutTheSoftDeleteInterface_HasNoSoftDeleteColumns()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ReserveHistory))!;

        var result = entityType.GetProperties().Select(property => property.Name).ToList();

        result.Should().NotContain(["IsDeleted", "DeletedAt"]);
    }

    [Fact]
    [Trait("Rule", "S:DATA-EfConfig")]
    public void Model_SqlServerKey_DefaultsToASequentialId()
    {
        var model = SqlServerModel.Create();
        var key = model.FindEntityType(typeof(CauseOfLossCode))!.FindProperty("Id")!;

        var result = key.GetDefaultValueSql();

        result.Should().Be("NEWSEQUENTIALID()");
    }

    [Fact]
    [Trait("Rule", "S:DATA-RowVer")]
    public void Model_AggregateRoot_HasAConcurrencyToken()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ReserveComponent))!;

        var result = entityType.FindProperty("RowVer")!.IsConcurrencyToken;

        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:DATA-RowVer")]
    public void Model_EntityThatIsNotARoot_HasNoRowVersion()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CauseOfLossCode))!;

        var result = entityType.FindProperty("RowVer");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Rule", "S:DATA-RowVer")]
    public void Model_SqlServerAggregateRoot_UsesARowVersionColumn()
    {
        var model = SqlServerModel.Create();
        var rowVersion = model.FindEntityType(typeof(ReserveComponent))!.FindProperty("RowVer")!;

        var result = rowVersion.GetColumnType();

        result.Should().Be("rowversion");
    }

    [Fact]
    [Trait("Rule", "S:DATA-Conventions")]
    public void Model_SqlServerTimestamps_AreStoredAsDateTimeOffset()
    {
        var model = SqlServerModel.Create();
        var createdAt = model.FindEntityType(typeof(CauseOfLossCode))!.FindProperty("CreatedAt")!;

        var result = createdAt.GetColumnType();

        result.Should().Be("datetimeoffset");
    }
}
