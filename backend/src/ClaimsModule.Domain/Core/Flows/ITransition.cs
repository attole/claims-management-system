namespace ClaimsModule.Domain.Core.Flows;

internal interface ITransition<TState>
{
    TState Target { get; }

    IReadOnlyCollection<TState> Sources { get; }
}
