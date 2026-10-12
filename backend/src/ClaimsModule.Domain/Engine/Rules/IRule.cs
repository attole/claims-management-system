namespace ClaimsModule.Domain.Engine.Rules;

public interface IRule<in TSubject>
{
    RuleResult Check(TSubject subject);
}
