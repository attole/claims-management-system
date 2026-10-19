using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Application.Results;

public static class OutcomeExtensions
{
    public static Result<TValue> ToResult<TValue>(this Outcome<TValue> outcome)
    {
        if (outcome.IsSuccess)
        {
            var warnings = outcome.WarningFindings
                .Select(ToWarning)
                .ToList();

            return Result<TValue>.Success(outcome.Value, warnings);
        }

        var errors = outcome.CriticalFindings
            .Select(ToError)
            .ToList();

        return Result<TValue>.Failure(errors);
    }

    private static Error ToError(Finding finding)
    {
        return Error.Validation(finding.Code, finding.Message, finding.Field);
    }

    private static Warning ToWarning(Finding finding)
    {
        return new Warning(finding.Code, finding.Message, finding.Field);
    }
}
