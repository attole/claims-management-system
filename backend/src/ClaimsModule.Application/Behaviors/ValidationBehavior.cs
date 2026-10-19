using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IFailableResult<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var validations = validators.Select(validator => validator.ValidateAsync(context, cancellationToken));
        var results = await Task.WhenAll(validations);
        var failures = results
            .SelectMany(result => result.Errors)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                .Select(failure => Error.Validation(failure.ErrorCode, failure.ErrorMessage, failure.PropertyName))
                .ToList();

            return TResponse.Failure(errors);
        }

        return await next(cancellationToken);
    }
}
