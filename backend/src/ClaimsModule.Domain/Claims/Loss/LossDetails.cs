namespace ClaimsModule.Domain.Claims.Loss;

public sealed record LossDetails(LossDate LossDate, string Description, string CauseOfLossCode)
{
    public const int MinimumDescriptionLength = 20;

    public static LossDetails Empty { get; } = new(LossDate.Empty, string.Empty, string.Empty);

    public string Location { get; init; } = string.Empty;

    public EstimatedLossAmount EstimatedLossAmount { get; init; } = EstimatedLossAmount.Empty;

    public string PoliceReportNumber { get; init; } = string.Empty;

    public string TrimmedDescription => Description.Trim();

    public bool HasLongEnoughDescription => TrimmedDescription.Length >= MinimumDescriptionLength;
}
