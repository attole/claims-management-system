using ClaimsModule.Domain.Reserves.Root;
using ClaimsModule.Domain.Reserves.Root.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.Reserves.Root;

public sealed class ReserveTests
{
    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Create_ReturnsAnActiveReserveOfTheRequestedType()
    {
        var type = new ReserveExpenseComponentType();
        var input = new OpenReserveInput(
            TestClaims.Organisation,
            TestReserves.ClaimId,
            type,
            "Expert fees",
            TestClaims.Actor,
            TestClaims.Start);

        var outcome = Reserve.Create(input);

        outcome.Value.ComponentType.Should().Be(type);
        outcome.Value.Status.Should().Be(ReserveStatus.Active);
    }
}
