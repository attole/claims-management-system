using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.Reserves.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.Reserves;

public sealed class ReserveTests
{
    [Fact]
    public void Reference_CarriesTheReserveItsClaimItsOrganisationAndItsType()
    {
        var type = new ReserveExpenseComponentType();
        var input = new OpenReserveInput(TestClaims.Organisation, TestReserves.ClaimId, type, "Expert fees", true);
        var reserve = Reserve.Open(input).Value;

        var result = reserve.Reference;

        result.Should().Be(new ReserveReference(reserve.Id, TestReserves.ClaimId, TestClaims.Organisation, type));
    }
}
