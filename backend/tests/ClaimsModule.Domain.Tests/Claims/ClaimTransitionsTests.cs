using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Domain.Tests.Claims;

public sealed class ClaimTransitionsTests
{
    [Theory]
    [Trait("Rule", "S:BR-ST-01")]
    [InlineData(ClaimStateKind.Draft, new[] { ClaimStateKind.Open })]
    [InlineData(
        ClaimStateKind.Open,
        new[]
        {
            ClaimStateKind.UnderInvestigation,
            ClaimStateKind.PendingPayment,
            ClaimStateKind.Closed,
            ClaimStateKind.Withdrawn,
        })]
    [InlineData(
        ClaimStateKind.UnderInvestigation,
        new[]
        {
            ClaimStateKind.Open,
            ClaimStateKind.PendingPayment,
            ClaimStateKind.Closed,
            ClaimStateKind.Withdrawn,
        })]
    [InlineData(ClaimStateKind.PendingPayment, new[] { ClaimStateKind.Closed })]
    [InlineData(ClaimStateKind.Closed, new[] { ClaimStateKind.Reopened })]
    [InlineData(ClaimStateKind.Reopened, new[] { ClaimStateKind.Open })]
    [InlineData(ClaimStateKind.Withdrawn, new ClaimStateKind[0])]
    public void NextFrom_ReturnsTheTargetsOfTheTransitionsThatAllowTheState(
        ClaimStateKind kind,
        ClaimStateKind[] expected)
    {
        var state = ClaimState.From(kind);

        var result = ClaimTransitions.NextFrom(state);

        result.Select(target => target.Marker).Should().Equal(expected);
    }
}
