using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class CloseSampleTransition : SampleTransition<CloseSampleTransition>
{
    public const string TextRequired = "TEXT_REQUIRED";

    public CloseSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom(new OpenSampleState())
            .Require(input => new DelegateSampleRule(_ => string.IsNullOrWhiteSpace(input.Text)
                ? CheckResult.Found(new Finding(TextRequired, TextRequired))
                : CheckResult.Pass));
    }
}
