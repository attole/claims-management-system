using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class SetLimitOverrideOperationTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-05")]
    public void Execute_ByAManager_SetsTheFlag()
    {
        var claim = TestClaims.Empty();
        var input = new SetLimitOverrideInput(true, TestClaims.Manager, TestClaims.Actor, TestClaims.Later);

        var outcome = new SetLimitOverrideOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.HasAggregateLimitOverride.Should().BeTrue();
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", "S:BR-R-05")]
    public void Execute_ByAManagerWithFalse_ClearsTheFlag()
    {
        var claim = TestClaims.WithLimitOverride();
        var input = new SetLimitOverrideInput(false, TestClaims.Manager, TestClaims.Actor, TestClaims.Later);

        new SetLimitOverrideOperation().Execute(claim, input);

        claim.HasAggregateLimitOverride.Should().BeFalse();
    }

    [Theory]
    [Trait("Rule", HasPermittedRole.Code)]
    [InlineData(0)]
    [InlineData(1)]
    public void Execute_ByAHandlerOrSupervisor_ReturnsTheFindingAndKeepsTheFlag(int roleIndex)
    {
        var claim = TestClaims.Empty();
        var role = roleIndex == 0 ? TestClaims.Handler : TestClaims.Supervisor;
        var input = new SetLimitOverrideInput(true, role, TestClaims.Actor, TestClaims.Later);

        var outcome = new SetLimitOverrideOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasPermittedRole.Code);
        claim.HasAggregateLimitOverride.Should().BeFalse();
    }
}
