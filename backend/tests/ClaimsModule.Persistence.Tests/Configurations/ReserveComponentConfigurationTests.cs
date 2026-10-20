using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ReserveComponentConfigurationTests
{
    [Fact]
    public void Configure_ComponentWithHistory_ReadsBackTheTypeStateAndRows()
    {
        using var database = new TestDatabase();
        var claimId = Ids.Next();
        using (var writer = database.CreateContext())
        {
            var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Expense);
            TestReserves.Submit(component, TestReserves.AutoApprovedAmount, database.Now);
            writer.ReserveComponents.Add(component);
            writer.SaveChanges();
        }

        using var context = database.CreateContext();

        var result = context.ReserveComponents.Include(component => component.History).Single();

        result.ComponentType.Should().Be(new ReserveExpenseComponentType());
        result.State.Should().Be(new ReserveComponentActiveState());
        result.History.Should().ContainSingle();
        result.ApprovedBalance.Should().Be(TestReserves.AutoApprovedAmount);
    }

    [Theory]
    [InlineData(ReserveComponentTypeKind.Indemnity, "Indemnity")]
    [InlineData(ReserveComponentTypeKind.Expense, "Expense")]
    [InlineData(ReserveComponentTypeKind.Alae, "Alae")]
    [InlineData(ReserveComponentTypeKind.SubrogationRecoverable, "SubrogationRecoverable")]
    public void Configure_ComponentType_IsStoredAsTheEnumMemberName(ReserveComponentTypeKind kind, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        context.ReserveComponents.Add(TestReserves.Open(Ids.Next(), TestDatabase.OrganisationId, kind));
        context.SaveChanges();

        var result = context.Database
            .SqlQueryRaw<string>("SELECT Component AS Value FROM ClaimReserveComponents")
            .Single();

        result.Should().Be(stored);
    }

    [Fact]
    public void Configure_ComponentState_IsStoredAsTheEnumMemberName()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var component = TestReserves.Open(Ids.Next(), TestDatabase.OrganisationId, ReserveComponentTypeKind.Alae);
        component.Close();
        context.ReserveComponents.Add(component);
        context.SaveChanges();

        var result = context.Database
            .SqlQueryRaw<string>("SELECT Status AS Value FROM ClaimReserveComponents")
            .Single();

        result.Should().Be("Closed");
    }
}
