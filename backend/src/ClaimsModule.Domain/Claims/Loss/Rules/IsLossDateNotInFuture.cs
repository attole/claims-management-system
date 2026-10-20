using System.Globalization;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsLossDateNotInFuture(DateTimeOffset now) : IRule<LossDetails>
{
    public const string Code = "S:BR-C-01";

    private const string Message = "Loss date cannot be in the future.";
    private const string Field = "LossDate";

    public CheckResult Check(LossDetails loss)
    {
        bool isNotInFuture = !loss.LossDate.IsAfter(now);
        string evidence = loss.LossDate.Value.ToString("O", CultureInfo.InvariantCulture);

        return isNotInFuture
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field) { Evidence = evidence });
    }
}
