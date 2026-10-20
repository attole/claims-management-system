using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.Flows;
using ClaimsModule.Domain.ReserveComponents.Inputs;
using ClaimsModule.Domain.ReserveComponents.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.Flows;

public sealed class OpenReserveComponentOperationTests
{
    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Run_CreatesAnActiveComponentOfTheRequestedType()
    {
        var type = ReserveComponentTypeKind.Expense;
        var claim = TestReserves.Claim(new ClaimOpenState());
        var input = new OpenReserveComponentInput(claim, type, "Expert fees");

        var outcome = OpenReserveComponentOperation.Run(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.ClaimId.Should().Be(TestReserves.ClaimId);
        outcome.Value.ComponentType.Marker.Should().Be(type);
        outcome.Value.State.Should().BeOfType<ReserveComponentActiveState>();
        outcome.Value.Notes.Should().Be("Expert fees");
        outcome.Value.OrganisationId.Should().Be(TestClaims.Organisation);
    }

    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Run_Twice_CreatesASecondComponentOfTheSameType()
    {
        var type = ReserveComponentTypeKind.Indemnity;
        var claim = TestReserves.Claim(new ClaimOpenState());
        var input = new OpenReserveComponentInput(claim, type, string.Empty);
        var first = OpenReserveComponentOperation.Run(input).Value;

        var outcome = OpenReserveComponentOperation.Run(input);

        outcome.Value.Id.Should().NotBe(first.Id);
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Run_OnAClosedOrWithdrawnClaim_ReturnsTheFinding()
    {
        var type = ReserveComponentTypeKind.Expense;
        var claim = TestReserves.Claim(new ClaimClosedState());
        var input = new OpenReserveComponentInput(claim, type, "Expert fees");

        var outcome = OpenReserveComponentOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingReserves.Code);
    }

    [Fact]
    [Trait("Rule", IsComponentTypeKnown.Code)]
    public void Run_WithAnUnknownComponentType_ReturnsTheFinding()
    {
        var claim = TestReserves.Claim(new ClaimOpenState());
        var input = new OpenReserveComponentInput(claim, (ReserveComponentTypeKind)99, string.Empty);

        var outcome = OpenReserveComponentOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsComponentTypeKnown.Code);
    }
}
