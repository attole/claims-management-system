using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal abstract class SampleFlow<TSelf> : Flow<TSelf, SampleSubject, SampleInput, string, DomainFinding>
    where TSelf : SampleFlow<TSelf>, new()
{
    protected override string Apply(SampleSubject subject, SampleInput input, CheckResult<DomainFinding> result)
    {
        subject.Log.Add("apply");

        return input.Text;
    }
}
