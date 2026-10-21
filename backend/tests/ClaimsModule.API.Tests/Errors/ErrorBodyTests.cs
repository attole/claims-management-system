using ClaimsModule.API.Errors;
using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ClaimsModule.API.Tests.Errors;

public sealed class ErrorBodyTests
{
    [Theory]
    [InlineData(ErrorKind.Validation, 422, "ValidationError")]
    [InlineData(ErrorKind.NotFound, 404, "NotFoundError")]
    [InlineData(ErrorKind.Forbidden, 403, "ForbiddenError")]
    [InlineData(ErrorKind.Conflict, 409, "ConflictError")]
    [Trait("Rule", "S:API-ErrorShape")]
    public void From_ErrorOfAKind_UsesItsStatusAndTypeName(ErrorKind kind, int status, string type)
    {
        var body = ErrorBody.From([new Error("CODE", "message", kind)]);

        body.Status.Should().Be(status);
        body.Type.Should().Be(type);
    }

    [Fact]
    [Trait("Rule", "S:API-ErrorShape")]
    public void From_ErrorsOnTheSameField_GroupsTheirMessages()
    {
        var errors = new List<Error>
        {
            Error.Validation("ONE", "first", "LossDate"),
            Error.Validation("TWO", "second", "LossDate"),
            Error.Validation("THREE", "third", "ClaimParties"),
        };

        var body = ErrorBody.From(errors);

        body.Errors["LossDate"].Should().Equal("first", "second");
        body.Errors["ClaimParties"].Should().Equal("third");
    }

    [Fact]
    [Trait("Rule", "S:API-ErrorShape")]
    public void From_ErrorWithoutAField_GoesUnderTheGeneralKey()
    {
        var body = ErrorBody.From([Error.Conflict("CODE", "the claim is closed")]);

        body.Errors[ErrorBody.GeneralKey].Should().Equal("the claim is closed");
    }

    [Fact]
    public void From_ErrorsOfDifferentKinds_UsesTheKindOfTheFirstError()
    {
        var body = ErrorBody.From([Error.NotFound("ONE", "missing"), Error.Validation("TWO", "bad")]);

        body.Status.Should().Be(404);
    }

    [Fact]
    public void From_AnError_DoesNotPutTheErrorCodeOnTheWire()
    {
        var body = ErrorBody.From([Error.Validation("SECRET_CODE", "message", "Field")]);

        body.Errors.Values.SelectMany(messages => messages).Should().NotContain("SECRET_CODE");
    }

    [Fact]
    public void Create_AKindAndAMessage_BuildsAGeneralError()
    {
        var body = ErrorBody.Create(ErrorKind.Forbidden, "not allowed");

        body.Errors[ErrorBody.GeneralKey].Should().Equal("not allowed");
    }

    [Fact]
    public void Unauthorized_Is401WithItsOwnTypeName()
    {
        var body = ErrorBody.Unauthorized();

        body.Should().BeEquivalentTo(new { Status = 401, Type = "UnauthorizedError" });
    }

    [Fact]
    [Trait("Rule", "S:API-ErrorShape")]
    public void FromModelState_InvalidFields_BecomesAValidationBodyByField()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "The Name field is required.");
        modelState.AddModelError(string.Empty, "A non-empty request body is required.");

        var body = ErrorBody.FromModelState(modelState);

        body.Status.Should().Be(422);
        body.Errors["Name"].Should().Equal("The Name field is required.");
        body.Errors[ErrorBody.GeneralKey].Should().Equal("A non-empty request body is required.");
    }

    [Fact]
    public void FromModelState_JsonPathKeys_AreShownWithoutThePathPrefix()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("$.name", "The JSON value could not be converted.");

        var body = ErrorBody.FromModelState(modelState);

        body.Errors.Keys.Should().Equal("name");
    }

    [Fact]
    public void ToResult_UsesTheStatusOfTheBody()
    {
        var result = ErrorBody.Create(ErrorKind.Conflict, "message").ToResult();

        result.StatusCode.Should().Be(409);
    }
}
