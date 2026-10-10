using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ClaimsModule.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = await CollectFailuresAsync(request, cancellationToken);

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }

    private async Task<List<ValidationFailure>> CollectFailuresAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        return results
            .SelectMany(result => result.Errors)
            .ToList();
    }
}
