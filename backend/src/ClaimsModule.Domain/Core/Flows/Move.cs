namespace ClaimsModule.Domain.Core.Flows;

public sealed record Move<TState>(TState From, TState To);
