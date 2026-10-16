namespace ClaimsModule.Domain.Common;

internal static class InputActor
{
    private const string MissingActorMessage = "An input needs the user who made the decision.";

    public static Guid Required(Guid actorId)
    {
        bool isSet = actorId != Guid.Empty;

        return isSet
            ? actorId
            : throw new ArgumentException(MissingActorMessage, nameof(actorId));
    }
}
