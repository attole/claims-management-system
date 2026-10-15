using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Flows;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Flows;

public sealed class UpdateNotesOperationTests
{
    [Fact]
    public void UpdateNotes_ReplacesTheNotes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new UpdateNotesInput(
            "called the insured",
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        UpdateNotesOperation.Execute(claim, input);

        claim.Notes.Should().Be("called the insured");
    }

    [Fact]
    public void UpdateNotes_WhenTheClaimIsClosed_ReplacesTheNotes()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new UpdateNotesInput("late call", TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = UpdateNotesOperation.Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.Notes.Should().Be("late call");
    }

    [Fact]
    [Trait("Rule", IsClaimNotWithdrawn.Code)]
    public void UpdateNotes_WhenTheClaimIsWithdrawn_Fails()
    {
        var claim = TestClaims.InState<ClaimWithdrawnState>();
        var input = new UpdateNotesInput("late call", TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = UpdateNotesOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimNotWithdrawn.Code);
    }
}
