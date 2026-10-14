using ClaimsModule.Domain.Claims.Root.Operations;
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
}
