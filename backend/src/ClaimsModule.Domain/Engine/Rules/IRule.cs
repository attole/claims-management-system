using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Engine.Rules;

public interface IRule<in TSubject>
{
    RuleResult Check(TSubject subject, OperationContext context);
}
