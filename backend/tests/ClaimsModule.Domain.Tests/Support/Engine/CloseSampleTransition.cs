using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Rules;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed class CloseSampleTransition : SampleTransition
{
    public const string TextRequired = "TEXT_REQUIRED";

    public CloseSampleTransition()
        : base(new ClosedSampleState())
    {
        From(new OpenSampleState())
            .Require(input => new DelegateRule((_, _) => string.IsNullOrWhiteSpace(input.Text)
                ? RuleResult.Fail(new RuleViolation(TextRequired, TextRequired))
                : RuleResult.Pass));
    }
}
