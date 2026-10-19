using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;

namespace ClaimsModule.Application.Tests.ReserveComponents;

public sealed class ReserveRequestTests
{
    [Fact]
    public void ToOpenInput_MapsTheComponentTypeAndTheClaimReference()
    {
        var claim = ApplicationClaims.Opened();
        var request = new ReserveRequest(ReserveTransactionType.Add, 1000m, "reason")
        {
            ComponentType = ReserveComponentTypeKind.Alae,
        };

        var result = request.ToOpenInput(claim.Reference);

        result.ComponentType.Name.Should().Be("Alae");
        result.Claim.ClaimId.Should().Be(claim.Id);
    }

    [Fact]
    public void ToSubmitInput_CarriesTheClaimFactsTheActorAndTheTime()
    {
        var claim = ApplicationClaims.Opened();
        var actorId = Guid.NewGuid();
        var request = new ReserveRequest(ReserveTransactionType.Adjust, -50m, "reason");

        var result = request.ToSubmitInput(claim, 700m, actorId, ClaimHandlerContext.Now);

        result.TransactionType.Should().Be(ReserveTransactionType.Adjust);
        result.Amount.Should().Be(-50m);
        result.ClaimApprovedTotal.Should().Be(700m);
        result.IsPolicyLinked.Should().BeTrue();
        result.ActorId.Should().Be(actorId);
    }

    [Fact]
    public void ToSubmitInput_WithNullText_UsesEmptyText()
    {
        var claim = ApplicationClaims.Opened();
        var request = new ReserveRequest(ReserveTransactionType.Add, 10m, null!);

        var result = request.ToSubmitInput(claim, 0m, Guid.NewGuid(), ClaimHandlerContext.Now);

        result.ChangeReason.Should().BeEmpty();
    }
}
