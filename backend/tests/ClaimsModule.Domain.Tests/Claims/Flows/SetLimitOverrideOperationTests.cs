using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Roles.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class SetLimitOverrideOperationTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-05")]
    public void Run_ByAManager_SetsTheFlag()
    {
        var claim = TestClaims.Empty();
        var input = new SetLimitOverrideInput(
            true,
            TestClaims.Manager,
            0m,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = SetLimitOverrideOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.HasAggregateLimitOverride.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:BR-R-05")]
    public void Run_ByAManagerWithFalse_ClearsTheFlag()
    {
        var claim = TestClaims.WithLimitOverride();
        var input = new SetLimitOverrideInput(
            false,
            TestClaims.Manager,
            0m,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        SetLimitOverrideOperation.Run(claim, input);

        claim.HasAggregateLimitOverride.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", HasMinimumRole.Code)]
    public void Run_ByASupervisor_ReturnsTheFindingAndKeepsTheFlag()
    {
        var claim = TestClaims.Empty();
        var input = new SetLimitOverrideInput(
            true,
            TestClaims.Supervisor,
            0m,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = SetLimitOverrideOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasMinimumRole.Code);
        claim.HasAggregateLimitOverride.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_OnAClosedClaim_ReturnsTheFindingAndKeepsTheFlag()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new SetLimitOverrideInput(
            true,
            TestClaims.Manager,
            0m,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = SetLimitOverrideOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
        claim.HasAggregateLimitOverride.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsOverrideRevocable.Code)]
    public void Run_RemovingItAboveTheLimit_ReturnsTheFindingAndKeepsTheFlag()
    {
        var claim = TestClaims.WithLimitOverride();
        var input = new SetLimitOverrideInput(
            false,
            TestClaims.Manager,
            12_000_000m,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = SetLimitOverrideOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsOverrideRevocable.Code);
        claim.HasAggregateLimitOverride.Should().BeTrue();
    }
}
