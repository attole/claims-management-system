using ClaimsModule.Domain.Claims.StateChanges;

namespace ClaimsModule.Domain.Tests.Claims.StateChanges;

public sealed class StateChangeReasonTests
{
    [Fact]
    public void Empty_IsNeitherSetNorComplete()
    {
        var reason = StateChangeReason.Empty;

        reason.IsSet.Should().BeFalse();
        reason.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Reason_WithACodeAndATextAfterTrimming_IsSetAndComplete()
    {
        var reason = new StateChangeReason(" CODE ", " text ");

        reason.IsSet.Should().BeTrue();
        reason.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void Reason_WithOnlyACode_IsSetButNotComplete()
    {
        var reason = new StateChangeReason("CODE", " ");

        reason.IsSet.Should().BeTrue();
        reason.IsComplete.Should().BeFalse();
    }
}
