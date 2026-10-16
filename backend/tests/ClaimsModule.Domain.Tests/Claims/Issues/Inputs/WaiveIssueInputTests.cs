using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Inputs;

public sealed class WaiveIssueInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        var reference = TestClaims.LossReference();

        Func<WaiveIssueInput> act = () =>
            new WaiveIssueInput(Guid.NewGuid(), TestClaims.Supervisor, TestClaims.Actor, default, reference);

        act.Should().Throw<ArgumentException>();
    }
}
