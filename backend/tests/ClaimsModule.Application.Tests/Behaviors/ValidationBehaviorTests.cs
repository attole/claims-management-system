using ClaimsModule.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ClaimsModule.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public async Task Handle_WhenAValidatorFails_ThrowsAndSkipsTheHandler()
    {
        var validator = new Mock<IValidator<string>>();
        validator
            .Setup(mock => mock.ValidateAsync(It.IsAny<ValidationContext<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "required")]));
        var handlerCalled = false;
        var behavior = new ValidationBehavior<string, string>([validator.Object]);
        var next = CreateNext(() => handlerCalled = true);

        Func<Task<string>> act = () => behavior.Handle("request", next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        handlerCalled.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public async Task Handle_WhenNoValidatorFails_CallsTheHandler()
    {
        var handlerCalled = false;
        var behavior = new ValidationBehavior<string, string>([]);

        var result = await behavior.Handle("request", CreateNext(() => handlerCalled = true), CancellationToken.None);

        result.Should().Be("ok");
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
