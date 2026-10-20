using ClaimsModule.Application.Results;

namespace ClaimsModule.Infrastructure.BackgroundJobs;

internal static class JobResult
{
    public static void ThrowIfFailed(Result result)
    {
        if (!result.IsSuccess)
        {
            string codes = string.Join(", ", result.Errors.Select(error => error.Code));

            throw new InvalidOperationException($"The job command was refused: {codes}.");
        }
    }
}
