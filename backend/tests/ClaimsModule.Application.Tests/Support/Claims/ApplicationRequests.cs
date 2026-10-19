using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.RiskObjects;

namespace ClaimsModule.Application.Tests.Support.Claims;

public static class ApplicationRequests
{
    public static LossRequest Loss()
    {
        var lossDate = new DateTimeOffset(2026, 10, 9, 8, 0, 0, TimeSpan.Zero);

        return new LossRequest(lossDate, "Water damage in the kitchen after a burst pipe", "WATER");
    }

    public static PartyRequest Claimant()
    {
        return new PartyRequest(PartyRoleKind.Claimant, PartyTypeKind.Person)
        {
            FirstName = "Ada",
            LastName = "Stone",
        };
    }

    public static PartyRequest Witness()
    {
        return new PartyRequest(PartyRoleKind.Witness, PartyTypeKind.Person)
        {
            FirstName = "Bo",
            LastName = "Reed",
        };
    }

    public static RiskObjectRequest Kitchen()
    {
        return new RiskObjectRequest(AssetType.Property, "Kitchen");
    }

    public static RiskObjectRequest Garage()
    {
        return new RiskObjectRequest(AssetType.Property, "Garage");
    }
}
