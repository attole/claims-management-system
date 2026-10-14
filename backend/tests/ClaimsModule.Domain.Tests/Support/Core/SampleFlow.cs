using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleFlow : Flow<SampleSubject, SampleInput, string, DomainFinding>
{
    private readonly FlowDecision _whenPreconditionFails;
    private readonly FlowDecision _whenBlocked;
    private readonly Func<SampleInput, IReadOnlyList<DomainFinding>> _early;

    public SampleFlow(
        FlowDecision whenPreconditionFails,
        FlowDecision whenBlocked,
        Func<SampleInput, IReadOnlyList<DomainFinding>> early,
        IEnumerable<IRule<SampleSubject>> rules)
    {
        _whenPreconditionFails = whenPreconditionFails;
        _whenBlocked = whenBlocked;
        _early = early;

        foreach (var rule in rules)
        {
            Require(rule);
        }
    }

    protected override FlowDecision WhenPreconditionFails => _whenPreconditionFails;

    protected override FlowDecision WhenBlocked => _whenBlocked;

    protected override IReadOnlyList<DomainFinding> CheckPreconditions(SampleSubject subject, SampleInput input)
    {
        return _early(input);
    }

    protected override string Apply(SampleSubject subject, SampleInput input, CheckResult<DomainFinding> result)
    {
        subject.Log.Add("apply");

        return input.Text;
    }

    protected override void AfterApply(SampleSubject subject, SampleInput input)
    {
        subject.Log.Add("after");
    }
}
