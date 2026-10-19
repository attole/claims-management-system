using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Results;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ClaimsModule.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public async Task Handle_WhenAValidatorFails_ReturnsValidationErrorsAndSkipsTheHandler()
    {
        var failure = new ValidationFailure("Name", "required") { ErrorCode = "NAME_REQUIRED" };
        var validator = new Mock<IValidator<string>>();
        validator
            .Setup(mock => mock.ValidateAsync(It.IsAny<ValidationContext<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));
        var handlerCalled = false;
        var behavior = new ValidationBehavior<string, Result<string>>([validator.Object]);
        var next = CreateNext(() => handlerCalled = true);

        var result = await behavior.Handle("request", next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Equal(Error.Validation("NAME_REQUIRED", "required", "Name"));
        handlerCalled.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public async Task Handle_WhenNoValidatorFails_CallsTheHandler()
    {
        var handlerCalled = false;
        var behavior = new ValidationBehavior<string, Result<string>>([]);

        var result = await behavior.Handle("request", CreateNext(() => handlerCalled = true), CancellationToken.None);

        result.Value.Should().Be("ok");
        handlerCalled.Should().BeTrue();
    }

    private static RequestHandlerDelegate<Result<string>> CreateNext(Action onCalled)
    {
        return _ =>
        {
            onCalled();

            return Task.FromResult(Result<string>.Success("ok"));
        };
    }
}
