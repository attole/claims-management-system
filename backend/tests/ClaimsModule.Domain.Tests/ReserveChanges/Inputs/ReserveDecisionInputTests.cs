using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Inputs;

public sealed class ReserveDecisionInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        Func<ReserveDecisionInput> act = () => new ReserveDecisionInput(TestReserves.Approver, DateTimeOffset.MinValue);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActorId_WhenNotGiven_Throws()
    {
        Func<ReserveDecisionInput> act = () => new ReserveDecisionInput(Guid.Empty, TestClaims.Later);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReviewerId_IsTheActor()
    {
        var input = new ReserveDecisionInput(TestReserves.Approver, TestClaims.Later);

        input.ReviewerId.Should().Be(TestReserves.Approver);
    }
}
