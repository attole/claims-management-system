using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleFlow : Flow<SampleSubject, SampleInput, string, DomainFinding>
{
    private readonly FlowDecision _whenBlocked;

    public SampleFlow(FlowDecision whenBlocked, IEnumerable<IRule<SampleSubject>> rules)
    {
        _whenBlocked = whenBlocked;

        foreach (var rule in rules)
        {
            Require(rule);
        }
    }

    protected override FlowDecision WhenBlocked => _whenBlocked;

    protected override string Apply(SampleSubject subject, SampleInput input, CheckResult<DomainFinding> result)
    {
        subject.Log.Add("apply");

        return input.Text;
    }
}
