using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed class SampleOperation : Operation<SampleSubject, SampleInput, string>
{
    public SampleOperation(
        IEnumerable<IRule<SampleSubject>> rules,
        IEnumerable<Func<SampleInput, IRule<SampleSubject>>> factories)
    {
        foreach (IRule<SampleSubject> rule in rules)
        {
            Require(rule);
        }

        foreach (Func<SampleInput, IRule<SampleSubject>> factory in factories)
        {
            Require(factory);
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
