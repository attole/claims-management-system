using ClaimsModule.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ClaimsModule.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public async Task Handle_ThrowsAndSkipsHandler_WhenAValidatorFails()
    {
        var validator = new Mock<IValidator<string>>();
        validator
            .Setup(mock => mock.ValidateAsync(It.IsAny<ValidationContext<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "required")]));
        var handlerCalled = false;
        var behavior = new ValidationBehavior<string, string>([validator.Object]);

        var act = () => behavior.Handle("request", CreateNext(() => handlerCalled = true), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        handlerCalled.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public async Task Handle_CallsHandler_WhenNoValidatorReportsFailures()
    {
        var handlerCalled = false;
        var behavior = new ValidationBehavior<string, string>([]);

        var response = await behavior.Handle("request", CreateNext(() => handlerCalled = true), CancellationToken.None);

        response.Should().Be("ok");
        handlerCalled.Should().BeTrue();
    }

    private static RequestHandlerDelegate<string> CreateNext(Action onCalled)
    {
        return _ =>
        {
            onCalled();
            return Task.FromResult("ok");
        };
    }
}
