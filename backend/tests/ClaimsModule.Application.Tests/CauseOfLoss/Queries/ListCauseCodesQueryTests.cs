using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.CauseOfLoss.Queries;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.CauseOfLoss;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.CauseOfLoss.Queries;

public sealed class ListCauseCodesQueryTests
{
    [Fact]
    [Trait("Rule", "S:API-GET-causes")]
    public async Task Handle_PassesThePerilCategoryToTheReader()
    {
        var code = new CauseCodeDto { Code = "WATER", Name = "Water damage", PerilCategory = "Property" };
        var context = new QueryContext().WithCauseCodes(code);
        var handler = new ListCauseCodesQueryHandler(context.Causes);
        var query = new ListCauseCodesQuery { PerilCategory = PerilCategory.Property };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Should().Equal(code);
        context.VerifyCausesListed(PerilCategory.Property);
    }

    [Fact]
    public async Task Handle_WithoutAFilter_AsksForEveryActiveCode()
    {
        var context = new QueryContext().WithCauseCodes();
        var handler = new ListCauseCodesQueryHandler(context.Causes);
        var query = new ListCauseCodesQuery();

        await handler.Handle(query, CancellationToken.None);

        context.VerifyCausesListed();
    }

    [Fact]
    public void Validate_WithAnUnknownPerilCategory_FailsOnTheField()
    {
        var validator = new ListCauseCodesQueryValidator();
        var query = new ListCauseCodesQuery { PerilCategory = (PerilCategory)99 };

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.PerilCategory);
    }

    [Fact]
    public void Validate_WithoutAFilter_HasNoErrors()
    {
        var validator = new ListCauseCodesQueryValidator();
        var query = new ListCauseCodesQuery();

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
