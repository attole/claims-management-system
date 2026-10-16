using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Inputs;

public sealed class ClaimTransitionInputTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("Settled outside the system", true)]
    public void HasNote_IsTrueOnlyForAWrittenNote(string note, bool isExpected)
    {
        var input = new ClaimTransitionInput(TestClaims.LossReference()) { Note = note };

        input.HasNote.Should().Be(isExpected);
    }
}
