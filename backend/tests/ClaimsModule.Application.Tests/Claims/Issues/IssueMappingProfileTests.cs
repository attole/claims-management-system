using AutoMapper;
using ClaimsModule.Application.Claims.Issues;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.Claims.Issues;

public sealed class IssueMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_Issue_FlattensTheFindingAndTheState()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var issue = ApplicationClaims.Draft().Issues[0];

        var result = mapper.Map<IssueDto>(issue);

        result.Code.Should().Be(issue.Finding.Code);
        result.State.Should().Be("Raised");
        result.Severity.Should().Be(issue.Finding.Severity.ToString());
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<IssueMappingProfile>());
    }
}
