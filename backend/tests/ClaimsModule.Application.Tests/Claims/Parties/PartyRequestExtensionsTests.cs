using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.Claims.Parties;

public sealed class PartyRequestExtensionsTests
{
    [Fact]
    public void ToDetails_MapsTheClosedSetsAndTheNames()
    {
        var request = ApplicationRequests.Claimant();

        var result = request.ToDetails();

        result.Role.Name.Should().Be("Claimant");
        result.Type.Name.Should().Be("Person");
        result.FirstName.Should().Be("Ada");
        result.LastName.Should().Be("Stone");
    }

    [Fact]
    public void ToDetails_WithNullOptionalText_UsesEmptyText()
    {
        var request = ApplicationRequests.Claimant() with
        {
            Email = null!,
            Phone = null!,
            Notes = null!,
        };

        var result = request.ToDetails();

        result.Email.Should().BeEmpty();
        result.Phone.Should().BeEmpty();
        result.Notes.Should().BeEmpty();
    }
}
