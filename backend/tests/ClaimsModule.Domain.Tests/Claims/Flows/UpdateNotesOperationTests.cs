using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class UpdateNotesOperationTests
{
    [Fact]
    public void Run_ReplacesTheNotes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new UpdateNotesInput("called the insured", TestClaims.LossReference());

        UpdateNotesOperation.Run(claim, input);

        claim.Notes.Should().Be("called the insured");
    }

    public static TheoryData<Claim> FrozenClaims => new()
    {
        TestClaims.InState<ClaimClosedState>(),
        TestClaims.InState<ClaimWithdrawnState>(),
    };

    [Theory]
    [MemberData(nameof(FrozenClaims))]
    public void Run_WhenTheClaimIsClosedOrWithdrawn_ReplacesTheNotes(Claim claim)
    {
        var input = new UpdateNotesInput("late call", TestClaims.LossReference());

        var outcome = UpdateNotesOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.Notes.Should().Be("late call");
    }
}
