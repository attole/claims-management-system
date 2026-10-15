using ClaimsModule.Domain.Reserves.Root;
using ClaimsModule.Domain.Reserves.Root.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.Reserves.Root.Operations;

public sealed class OpenReserveOperationTests
{
    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Execute_CreatesAnActiveComponentOfTheRequestedType()
    {
        var type = new ReserveExpenseComponentType();
        var input = new OpenReserveInput(
            TestClaims.Organisation,
            TestReserves.ClaimId,
            type,
            "Expert fees",
            TestClaims.Actor,
            TestClaims.Start);

        var outcome = new OpenReserveOperation().Execute(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.ClaimId.Should().Be(TestReserves.ClaimId);
        outcome.Value.ComponentType.Should().Be(type);
        outcome.Value.Status.Should().Be(ReserveStatus.Active);
        outcome.Value.Notes.Should().Be("Expert fees");
        outcome.Value.OrganisationId.Should().Be(TestClaims.Organisation);
    }

    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Execute_Twice_CreatesASecondComponentOfTheSameType()
    {
        var type = new ReserveIndemnityComponentType();
        var input = new OpenReserveInput(
            TestClaims.Organisation,
            TestReserves.ClaimId,
            type,
            string.Empty,
            TestClaims.Actor,
            TestClaims.Start);
        var operation = new OpenReserveOperation();
        var first = operation.Execute(input).Value;

        var outcome = operation.Execute(input);

        outcome.Value.Id.Should().NotBe(first.Id);
    }
}
