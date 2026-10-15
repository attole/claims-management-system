namespace ClaimsModule.Domain.Core.Base.Checks;

internal interface ICheck<in TSubject, TFinding>
    where TFinding : IFinding
{
    CheckResult<TFinding> Check(TSubject subject);
}
