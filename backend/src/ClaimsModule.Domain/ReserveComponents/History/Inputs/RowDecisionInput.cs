namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

internal record RowDecisionInput(Guid ActorId, DateTimeOffset Now)
{
    public string Reason { get; init; } = string.Empty;

    public virtual Guid ReviewerId => ActorId;
}
