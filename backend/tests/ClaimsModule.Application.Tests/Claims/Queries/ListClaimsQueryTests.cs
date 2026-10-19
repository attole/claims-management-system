using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Claims.Queries;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Queries;

public sealed class ListClaimsQueryTests
{
    [Fact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task Handle_PassesTheFilterAndThePageToTheReaderAndReturnsItsPage()
    {
        var summary = new ClaimSummaryDto { ClaimNumber = "CLM-2026-0000001" };
        var page = new PagedResult<ClaimSummaryDto>([summary], 2, 20, 41);
        var context = new QueryContext().WithClaimPage(page);
        var handler = new ListClaimsQueryHandler(context.Claims);
        var handlerId = Guid.NewGuid();
        var query = new ListClaimsQuery(2, 20)
        {
            Status = ClaimStateKind.Open,
            AssignedHandlerId = handlerId,
            Search = "Stone",
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Should().Be(page);
        context.VerifyClaimListed(
            filter => filter.Statuses.SequenceEqual(new[] { ClaimStateKind.Open })
                && filter.AssignedHandlerId == handlerId
                && filter.Search == "Stone",
            new PageWindow(2, 20));
    }

    [Fact]
    public void ToFilter_WithoutStatusOrDates_UsesAnEmptyListAndOpenBounds()
    {
        var query = new ListClaimsQuery(1, 10);

        var result = query.ToFilter();

        result.Statuses.Should().BeEmpty();
        result.DateFrom.Should().Be(DateOnly.MinValue);
        result.DateTo.Should().Be(DateOnly.MaxValue);
    }

    [Fact]
    public void ToFilter_WithNullText_UsesEmptyText()
    {
        var query = new ListClaimsQuery(1, 10)
        {
            CauseOfLossCode = null!,
            Search = null!,
        };

        var result = query.ToFilter();

        result.CauseOfLossCode.Should().BeEmpty();
        result.Search.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ForAValidQuery_HasNoErrors()
    {
        var validator = new ListClaimsQueryValidator();
        var query = new ListClaimsQuery(1, 20)
        {
            Status = ClaimStateKind.Closed,
            DateFrom = new DateOnly(2026, 1, 1),
            DateTo = new DateOnly(2026, 12, 31),
        };

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnUnknownStatus_FailsOnTheField()
    {
        var validator = new ListClaimsQueryValidator();
        var query = new ListClaimsQuery(1, 20) { Status = (ClaimStateKind)99 };

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.Status);
    }

    [Fact]
    public void Validate_WithAnEndDateBeforeTheStartDate_FailsOnTheEndDate()
    {
        var validator = new ListClaimsQueryValidator();
        var query = new ListClaimsQuery(1, 20)
        {
            DateFrom = new DateOnly(2026, 6, 1),
            DateTo = new DateOnly(2026, 5, 1),
        };

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.DateTo);
    }

    [Fact]
    public void Validate_WithATooLongSearch_FailsOnTheField()
    {
        var validator = new ListClaimsQueryValidator();
        var query = new ListClaimsQuery(1, 20) { Search = new string('x', ClaimFieldLengths.SearchText + 1) };

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.Search);
    }

    [Fact]
    public void Validate_WithAPageSizeOverTheLimit_FailsOnThePageSize()
    {
        var validator = new ListClaimsQueryValidator();
        var query = new ListClaimsQuery(1, PageLimits.MaxPageSize + 1);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.PageSize);
    }
}
