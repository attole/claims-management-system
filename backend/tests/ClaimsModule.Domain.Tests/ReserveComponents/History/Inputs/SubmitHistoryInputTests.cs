using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Inputs;

public sealed class SubmitHistoryInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        Func<SubmitHistoryInput> act = () => new SubmitHistoryInput(
            ReserveTransactionType.Add,
            1_000m,
            "Initial estimate",
            TestReserves.Submitter,
            new ClaimOpenState(),
            true,
            0m,
            default);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActorId_WhenNotGiven_Throws()
    {
        Func<SubmitHistoryInput> act = () => new SubmitHistoryInput(
            ReserveTransactionType.Add,
            1_000m,
            "Initial estimate",
            Guid.Empty,
            new ClaimOpenState(),
            true,
            0m,
            TestClaims.Start);

        act.Should().Throw<ArgumentException>();
    }
}
