using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.Reserves.Flows;
using ClaimsModule.Domain.Reserves.Inputs;
using ClaimsModule.Domain.Reserves.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.Reserves.Flows;

public sealed class OpenReserveOperationTests
{
    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Run_CreatesAnActiveComponentOfTheRequestedType()
    {
        var type = new ReserveExpenseComponentType();
        var input = new OpenReserveInput(TestClaims.Organisation, TestReserves.ClaimId, type, "Expert fees", true);

        var outcome = OpenReserveOperation.Run(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.ClaimId.Should().Be(TestReserves.ClaimId);
        outcome.Value.ComponentType.Should().Be(type);
        outcome.Value.Status.Should().Be(ReserveStatus.Active);
        outcome.Value.Notes.Should().Be("Expert fees");
        outcome.Value.OrganisationId.Should().Be(TestClaims.Organisation);
    }

    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Run_Twice_CreatesASecondComponentOfTheSameType()
    {
        var type = new ReserveIndemnityComponentType();
        var input = new OpenReserveInput(TestClaims.Organisation, TestReserves.ClaimId, type, string.Empty, true);
        var first = OpenReserveOperation.Run(input).Value;

        var outcome = OpenReserveOperation.Run(input);

        outcome.Value.Id.Should().NotBe(first.Id);
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Run_OnAClosedOrWithdrawnClaim_ReturnsTheFinding()
    {
        var type = new ReserveExpenseComponentType();
        var input = new OpenReserveInput(TestClaims.Organisation, TestReserves.ClaimId, type, "Expert fees", false);

        var outcome = OpenReserveOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingReserves.Code);
    }
}
