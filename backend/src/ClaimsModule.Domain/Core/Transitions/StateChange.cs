namespace ClaimsModule.Domain.Core.Transitions;

public sealed record StateChange<TState>(TState From, TState To);
