using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class HasPostingJob(string jobId) : IRule<object>
{
    public const string Code = "RESERVE_POSTING_JOB_REQUIRED";

    private const string Message = "A posting move needs the id of its job.";
    private const string Field = "JobId";

    public CheckResult Check(object subject)
    {
        bool hasJob = !string.IsNullOrWhiteSpace(jobId);

        return hasJob
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
