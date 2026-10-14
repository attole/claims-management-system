using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleCheckSet : CheckSet<SampleSubject, SampleInput, DomainFinding>
{
    public SampleCheckSet(
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
}
