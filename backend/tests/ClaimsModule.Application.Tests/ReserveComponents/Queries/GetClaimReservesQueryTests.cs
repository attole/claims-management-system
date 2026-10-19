using ClaimsModule.Application.Claims;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.ReserveComponents.Queries;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.Queries;

public sealed class GetClaimReservesQueryTests
{
    [Fact]
    [Trait("Rule", "S:API-GET-reserves")]
    public async Task Handle_ReturnsTheReservesOfAKnownClaim()
    {
        var claimId = Guid.NewGuid();
        var reserves = new ClaimReservesDto { ApprovedTotal = 750m };
        var context = new QueryContext().WithKnownClaim(claimId).WithReserves(reserves);
        var handler = new GetClaimReservesQueryHandler(context.Claims, context.Reserves);
        var query = new GetClaimReservesQuery(claimId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Should().Be(reserves);
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFound()
    {
        var claimId = Guid.NewGuid();
        var context = new QueryContext().WithUnknownClaim(claimId);
        var handler = new GetClaimReservesQueryHandler(context.Claims, context.Reserves);
        var query = new GetClaimReservesQuery(claimId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new GetClaimReservesQueryValidator();
        var query = new GetClaimReservesQuery(Guid.Empty);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }
}
