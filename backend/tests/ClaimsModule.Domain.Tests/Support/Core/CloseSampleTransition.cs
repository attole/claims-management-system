using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class CloseSampleTransition : SampleTransition
{
    public const string TextRequired = "TEXT_REQUIRED";

    public CloseSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom(new OpenSampleState())
            .Require(input => new DelegateRule(_ => string.IsNullOrWhiteSpace(input.Text)
                ? CheckResult<DomainFinding>.Found(new RuleFinding(TextRequired, TextRequired))
                : CheckResult<DomainFinding>.Pass));
    }
}
