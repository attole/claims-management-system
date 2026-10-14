using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleOperation : Operation<SampleSubject, SampleInput, string>
{
    public SampleOperation(
        IEnumerable<IRule<SampleSubject>> rules,
        IEnumerable<Func<SampleInput, IRule<SampleSubject>>> rulesFromInput)
    {
        foreach (var rule in rules)
        {
            Require(rule);
        }

        foreach (var ruleFromInput in rulesFromInput)
        {
            Require(ruleFromInput);
        }
    }

    public int ApplyCount { get; private set; }

    protected override string Apply(SampleSubject subject, SampleInput input)
    {
        ApplyCount++;
        subject.Log.Add($"apply {input.Text}");

        return input.Text;
    }
}
