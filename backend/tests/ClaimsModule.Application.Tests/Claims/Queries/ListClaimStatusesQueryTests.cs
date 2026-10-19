using ClaimsModule.Application.Claims.Queries;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Tests.Claims.Queries;

public sealed class ListClaimStatusesQueryTests
{
    [Fact]
    [Trait("Rule", "S:STATUS-Values")]
    public async Task Handle_ListsEveryStatus()
    {
        var handler = new ListClaimStatusesQueryHandler();
        var query = new ListClaimStatusesQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Select(info => info.Status).Should().Equal(
            "Draft",
            "Open",
            "UnderInvestigation",
            "PendingPayment",
            "Closed",
            "Reopened",
            "Withdrawn");
    }

    [Fact]
    [Trait("Rule", "S:API-GET-statuses")]
    public async Task Handle_GivesEachStatusTheNextStatusesOfTheDomainTransitions()
    {
        var handler = new ListClaimStatusesQueryHandler();
        var query = new ListClaimStatusesQuery();
        var expected = Enum.GetValues<ClaimStateKind>()
            .Select(kind => ClaimState.From(kind))
            .Select(state => Describe(state.Name, ClaimTransitions.NextFrom(state).Select(target => target.Name)))
            .ToList();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Select(info => Describe(info.Status, info.NextStatuses)).Should().Equal(expected);
    }

    private static string Describe(string status, IEnumerable<string> nextStatuses)
    {
        return $"{status}: {string.Join(", ", nextStatuses)}";
    }
}
