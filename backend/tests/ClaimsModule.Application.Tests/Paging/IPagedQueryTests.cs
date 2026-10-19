using ClaimsModule.Application.Paging;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Paging;

public sealed class IPagedQueryTests
{
    [Fact]
    public void Validate_ForAPageWithinTheLimits_HasNoErrors()
    {
        var validator = new SamplePagedQueryValidator();
        var query = new SamplePagedQuery(1, PageLimits.MaxPageSize);

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ForAPageBelowOne_FailsOnThePage()
    {
        var validator = new SamplePagedQueryValidator();
        var query = new SamplePagedQuery(0, 10);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(PageLimits.MaxPageSize + 1)]
    public void Validate_ForAPageSizeOutsideTheLimits_FailsOnThePageSize(int pageSize)
    {
        var validator = new SamplePagedQueryValidator();
        var query = new SamplePagedQuery(1, pageSize);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.PageSize);
    }

    private sealed record SamplePagedQuery(int Page, int PageSize) : IPagedQuery;

    private sealed class SamplePagedQueryValidator : PagedQueryValidator<SamplePagedQuery>;
}
