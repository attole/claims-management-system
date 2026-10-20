using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.ReserveComponents;

public static class ReserveErrors
{
    public const string ComponentNotFoundCode = "RESERVE_COMPONENT_NOT_FOUND";
    public const string HistoryNotFoundCode = "RESERVE_HISTORY_NOT_FOUND";

    public static Error ComponentNotFound(Guid componentId)
    {
        return Error.NotFound(ComponentNotFoundCode, $"Reserve component {componentId} does not exist on the claim.");
    }

    public static Error HistoryNotFound(Guid reserveHistoryId)
    {
        var message = $"Reserve transaction {reserveHistoryId} does not exist on the claim.";

        return Error.NotFound(HistoryNotFoundCode, message);
    }
}

public static class ReserveFieldLengths
{
    public const int ChangeReason = 500;
    public const int ReviewReason = 500;
}
