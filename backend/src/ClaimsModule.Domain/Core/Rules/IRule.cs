namespace ClaimsModule.Domain.Core.Rules;

internal interface IRule<in TSubject>
{
    CheckResult Check(TSubject subject);
}
