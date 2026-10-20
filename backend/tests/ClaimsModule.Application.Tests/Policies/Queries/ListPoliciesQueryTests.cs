using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Policies.Queries;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Policies.Queries;

public sealed class ListPoliciesQueryTests
{
    [Fact]
    [Trait("Rule", "S:API-GET-policies-search")]
    public async Task Handle_PassesTheTextAndThePageToTheReaderAndReturnsItsPage()
    {
        var policy = new PolicyDto { PolicyNumber = "POL-1001", CoverageTypes = ["Property", "Liability"] };
        var page = new PagedResult<PolicyDto>([policy], 1, 20, 1);
        var context = new QueryContext().WithPolicyPage(page);
        var handler = new ListPoliciesQueryHandler(context.Policies);
        var query = new ListPoliciesQuery("stone", 1, 20);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Should().Be(page);
        context.VerifyPoliciesSearched("stone", new PageWindow(1, 20));
    }

    [Fact]
    public void Validate_ForAValidQuery_HasNoErrors()
    {
        var validator = new ListPoliciesQueryValidator();
        var query = new ListPoliciesQuery("stone", 1, 20);

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullText_FailsOnTheField()
    {
        var validator = new ListPoliciesQueryValidator();
        var query = new ListPoliciesQuery(null!, 1, 20);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.Text);
    }

    [Fact]
    public void Validate_WithATooLongText_FailsOnTheField()
    {
        var validator = new ListPoliciesQueryValidator();
        var query = new ListPoliciesQuery(new string('x', PolicyFieldLengths.SearchText + 1), 1, 20);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.Text);
    }
}
