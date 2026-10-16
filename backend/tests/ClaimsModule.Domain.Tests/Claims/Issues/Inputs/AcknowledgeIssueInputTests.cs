using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Inputs;

public sealed class AcknowledgeIssueInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        Func<AcknowledgeIssueInput> act = () =>
            new AcknowledgeIssueInput(Guid.NewGuid(), TestClaims.Actor, default, TestClaims.LossReference());

        act.Should().Throw<ArgumentException>();
    }
}
