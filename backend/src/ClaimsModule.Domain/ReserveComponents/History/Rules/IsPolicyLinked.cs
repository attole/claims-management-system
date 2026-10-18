using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class IsPolicyLinked(bool isLinked) : IRule<object>
{
    public const string Code = "S:BR-C-06";

    private const string Message = "No policy linked. Policy must be associated before reserves can be set.";
    private const string Field = "PolicyId";

    public CheckResult Check(object subject)
    {
        bool isPolicyLinked = isLinked;

        return isPolicyLinked
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
