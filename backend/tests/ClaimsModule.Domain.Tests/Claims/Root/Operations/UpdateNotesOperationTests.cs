using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class UpdateNotesOperationTests
{
    [Fact]
    public void UpdateNotes_ReplacesTheNotes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new UpdateNotesInput("called the insured", TestClaims.Actor, TestClaims.Later);

        new UpdateNotesOperation().Execute(claim, input);

        claim.Notes.Should().Be("called the insured");
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void UpdateNotes_WhenTheClaimIsClosed_ReplacesTheNotes()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new UpdateNotesInput("late call", TestClaims.Actor, TestClaims.Later);

        var outcome = new UpdateNotesOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.Notes.Should().Be("late call");
    }

    [Fact]
    [Trait("Rule", IsClaimNotWithdrawn.Code)]
    public void UpdateNotes_WhenTheClaimIsWithdrawn_Fails()
    {
        var claim = TestClaims.InState(new ClaimWithdrawnState());
        var input = new UpdateNotesInput("late call", TestClaims.Actor, TestClaims.Later);

        var outcome = new UpdateNotesOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimNotWithdrawn.Code);
    }
}
