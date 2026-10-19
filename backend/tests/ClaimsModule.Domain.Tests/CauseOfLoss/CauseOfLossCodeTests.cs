using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.CauseOfLoss;

public sealed class CauseOfLossCodeTests
{
    [Fact]
    public void CauseOfLossCode_KeepsTheDataItWasBuiltWith()
    {
        var id = Guid.Parse("88888888-8888-8888-8888-888888888888");

        var result = new CauseOfLossCode(id, TestClaims.Organisation, "FIRE", "Fire", PerilCategory.Property, true);

        result.Id.Should().Be(id);
        result.OrganisationId.Should().Be(TestClaims.Organisation);
        result.Code.Should().Be("FIRE");
        result.Name.Should().Be("Fire");
        result.PerilCategory.Should().Be(PerilCategory.Property);
        result.IsActive.Should().BeTrue();
    }
}
