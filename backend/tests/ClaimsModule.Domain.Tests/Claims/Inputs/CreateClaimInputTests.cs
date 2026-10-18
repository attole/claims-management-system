using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Inputs;

public sealed class CreateClaimInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        var number = ClaimNumber.From("CLM-2026-0000001");
        var period = TestClaims.Period();
        var causeCodes = TestClaims.Codes();

        Func<CreateClaimInput> act = () =>
            new CreateClaimInput(TestClaims.Organisation, number, TestClaims.Start, default, period, causeCodes);

        act.Should().Throw<ArgumentException>();
    }
}
