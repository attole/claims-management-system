namespace ClaimsModule.Domain.Core.Base.Checks;

public interface ICheck<in TSubject, TFinding>
    where TFinding : IFinding
{
    CheckResult<TFinding> Check(TSubject subject);
}
