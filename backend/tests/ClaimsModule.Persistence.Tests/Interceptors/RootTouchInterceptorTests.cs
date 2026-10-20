using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Interceptors;

public sealed class RootTouchInterceptorTests
{
    private static readonly DateTimeOffset _later = new(2026, 3, 2, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_NewChildRow_MarksItsRootModified()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        using (var writer = database.CreateContext())
        {
            writer.ReserveComponents.Add(component);
            writer.SaveChanges();
        }

        database.Now = _later;
        using var context = database.CreateContext();
        var stored = context.ReserveComponents.Include(found => found.History).Single();
        TestReserves.Submit(stored, TestReserves.AutoApprovedAmount, _later);

        var result = context.SaveChanges();

        result.Should().Be(2);
        context.Entry(stored).Property("UpdatedAt").CurrentValue.Should().Be(_later);
        context.Entry(stored).Property("UserModified").CurrentValue.Should().Be(TestDatabase.UserId);
    }

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_NewPartyOnALoadedClaim_MarksTheClaimModified()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        database.Now = _later;
        using var context = database.CreateContext();
        var stored = context.Claims.Include(found => found.Parties).Single(found => found.Id == claimId);
        var witness = new PartyDetails(new PartyWitnessRole(), new PartyPersonType()) { FirstName = "Bo" };
        stored.AddParty(TestClaims.PartyInput(database, witness));

        context.SaveChanges();

        context.Entry(stored).Property("UpdatedAt").CurrentValue.Should().Be(_later);
    }

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_NewReserveRow_MarksTheComponentAndTheClaimModified()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        using (var writer = database.CreateContext())
        {
            writer.ReserveComponents.Add(component);
            writer.SaveChanges();
        }

        database.Now = _later;
        using var context = database.CreateContext();
        var claim = context.Claims.Single();
        var stored = context.ReserveComponents.Include(found => found.History).Single();
        TestReserves.Submit(stored, TestReserves.AutoApprovedAmount, _later);

        context.SaveChanges();

        context.Entry(claim).Property("UpdatedAt").CurrentValue.Should().Be(_later);
    }

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_UntouchedRoot_IsNotWritten()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var claimId = TestClaims.Seed(database);
        var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        context.ReserveComponents.Add(component);
        context.SaveChanges();

        var result = context.SaveChanges();

        result.Should().Be(0);
    }
}
