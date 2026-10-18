using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class HasPostingJobTests
{
    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Check_WithAJob_Passes()
    {
        var result = new HasPostingJob("job-1").Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Check_WithoutAJob_ReturnsTheFinding()
    {
        var result = new HasPostingJob(" ").Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasPostingJob.Code);
    }
}
