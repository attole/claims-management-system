namespace ClaimsModule.Domain.Core.Base.Checks;

internal class CheckSet<TSubject, TInput, TFinding>
    where TFinding : IFinding
{
    private readonly List<Func<TInput, ICheck<TSubject, TFinding>>> _checksFromInput = [];

    public CheckResult<TFinding> Evaluate(TSubject subject, TInput input)
    {
        var results = _checksFromInput.Select(checkFromInput => checkFromInput(input).Check(subject));

        return CheckResult<TFinding>.Combine(results);
    }

    protected internal void Require(ICheck<TSubject, TFinding> check)
    {
        _checksFromInput.Add(_ => check);
    }

    protected internal void Require(Func<TInput, ICheck<TSubject, TFinding>> checkFromInput)
    {
        _checksFromInput.Add(checkFromInput);
    }
}
