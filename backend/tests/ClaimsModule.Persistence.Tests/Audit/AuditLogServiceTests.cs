using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Persistence.Audit;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Audit;

public sealed class AuditLogServiceTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Correlation")]
    public async Task WriteAsync_Entry_AddsItWithTheCorrelationIdAndTheRelatedEntity()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var service = new AuditLogService(context);
        var correlationId = Guid.NewGuid();
        var related = Guid.NewGuid();
        var entry = new AuditEntry(claimId, AuditEventTypes.DocumentUploaded, "Document uploaded.")
        {
            OldValue = "{}",
            NewValue = "{\"a\":1}",
            RelatedEntityId = related,
            RelatedEntityType = "ClaimDocument",
        };

        await service.WriteAsync(entry, new AuditContext(database.Now, TestDatabase.UserId, correlationId), default);

        var result = context.AuditLog.Local.Single();
        result.ClaimId.Should().Be(claimId);
        result.OrganisationId.Should().Be(TestDatabase.OrganisationId);
        result.EventType.Should().Be(AuditEventTypes.DocumentUploaded);
        result.OldValue.Should().Be("{}");
        result.NewValue.Should().Be("{\"a\":1}");
        result.RelatedEntityId.Should().Be(related);
        result.RelatedEntityType.Should().Be("ClaimDocument");
        result.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task WriteAsync_EntryWithEmptyOptionalValues_AddsNulls()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var service = new AuditLogService(context);
        var entry = new AuditEntry(claimId, AuditEventTypes.ClaimCreated, "Claim created.");

        await service.WriteAsync(entry, new AuditContext(database.Now, Guid.Empty, Guid.Empty), default);

        var result = context.AuditLog.Local.Single();
        result.OldValue.Should().BeNull();
        result.NewValue.Should().BeNull();
        result.RelatedEntityId.Should().BeNull();
        result.RelatedEntityType.Should().BeNull();
        result.CorrelationId.Should().BeNull();
    }

    [Fact]
    public async Task WriteAsync_Entry_IsOnlyAddedToTheContext()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var service = new AuditLogService(context);
        var entry = new AuditEntry(claimId, AuditEventTypes.ClaimCreated, "Claim created.");

        await service.WriteAsync(entry, new AuditContext(database.Now, TestDatabase.UserId, Guid.NewGuid()), default);

        var result = context.Entry(context.AuditLog.Local.Single()).State;
        result.Should().Be(EntityState.Added);
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task WriteAsync_JobWithNoOrganisation_StoresTheEntryUnderTheClaimsOrganisation()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext(Guid.Empty, Guid.Empty);
        var service = new AuditLogService(context);
        var entry = new AuditEntry(claimId, AuditEventTypes.SlaBreachDetected, "Breach.");

        await service.WriteAsync(entry, new AuditContext(database.Now, Guid.Empty, Guid.NewGuid()), default);

        var result = context.AuditLog.Local.Single();
        result.OrganisationId.Should().Be(TestDatabase.OrganisationId);
    }

    [Fact]
    public async Task WriteAsync_ClaimAddedInTheSameUnit_UsesItsOrganisation()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var service = new AuditLogService(context);
        var claim = TestClaims.Create(database);
        context.Claims.Add(claim);
        var entry = new AuditEntry(claim.Id, AuditEventTypes.ClaimCreated, "Claim created.");

        await service.WriteAsync(entry, new AuditContext(database.Now, TestDatabase.UserId, Guid.NewGuid()), default);

        var result = context.AuditLog.Local.Single();
        result.OrganisationId.Should().Be(claim.OrganisationId);
    }
}
