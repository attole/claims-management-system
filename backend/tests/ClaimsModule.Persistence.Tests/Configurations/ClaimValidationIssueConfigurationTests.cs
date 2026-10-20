using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ClaimValidationIssueConfigurationTests
{
    [Fact]
    [Trait("Rule", "S:BR-C-05")]
    public void Configure_RaisedIssues_ReadBackTheFindingAndState()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Create(database);
        TestClaims.Save(database, written);
        using var context = database.CreateContext();

        var result = context.Set<ClaimValidationIssue>().ToList();

        result.Should().NotBeEmpty();
        result.Select(issue => issue.Finding).Should().BeEquivalentTo(written.Issues.Select(issue => issue.Finding));
        result.Should().OnlyContain(issue => issue.State == new IssueRaisedState());
    }

    [Theory]
    [InlineData(IssueStateKind.Raised, "Raised")]
    [InlineData(IssueStateKind.Cleared, "Cleared")]
    [InlineData(IssueStateKind.Acknowledged, "Acknowledged")]
    [InlineData(IssueStateKind.Waived, "Waived")]
    public void Configure_State_IsStoredAsTheEnumMemberName(IssueStateKind kind, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ClaimValidationIssue))!;
        var converter = entityType.FindProperty(nameof(ClaimValidationIssue.State))!.GetValueConverter()!;

        var result = converter.ConvertToProvider(IssueState.From(kind));

        result.Should().Be(stored);
    }
}
