using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.CauseOfLoss;
using Microsoft.EntityFrameworkCore;

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
    [Trait("Rule", "S:DATA-Naming")]
    public void Model_Tables_AreNamedAfterTheSpecification()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Model.GetEntityTypes()
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
            .SelectMany(entityType => entityType.GetProperties())
            .Select(property => property.GetColumnName())
            .ToList();

        result.Should().OnlyContain(name => char.IsUpper(name[0]) && !name.Contains('_'));
    }
}
