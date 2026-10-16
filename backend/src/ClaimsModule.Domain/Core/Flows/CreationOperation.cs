namespace ClaimsModule.Domain.Core.Flows;

internal abstract class CreationOperation<TSelf, TInput, TValue> : Operation<TSelf, NoSubject, TInput, TValue>
    where TSelf : CreationOperation<TSelf, TInput, TValue>, new()
{
    public static Outcome<TValue> Run(TInput input)
    {
        return Run(NoSubject.Value, input);
    }

    protected abstract Outcome<TValue> Apply(TInput input);

    protected sealed override Outcome<TValue> Apply(NoSubject subject, TInput input)
    {
        return Apply(input);
    }
}
