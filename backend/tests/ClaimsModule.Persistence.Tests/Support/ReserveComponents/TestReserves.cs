using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.Inputs;

namespace ClaimsModule.Persistence.Tests.Support.ReserveComponents;

internal static class TestReserves
{
    public const decimal PendingAmount = 50_000m;

    public const decimal AutoApprovedAmount = 5_000m;

    public static ReserveComponent Open(Guid claimId, Guid organisationId, ReserveComponentTypeKind kind)
    {
        var claim = new ClaimReference(claimId, organisationId, new ClaimOpenState());
        var input = new OpenReserveComponentInput(claim, kind, "Opening estimate");

        return ReserveComponent.Open(input).Value;
    }

    public static ReserveHistory Submit(ReserveComponent component, decimal amount, DateTimeOffset now)
    {
        var input = new SubmitHistoryInput(
            ReserveTransactionType.Add,
            amount,
            "Initial estimate",
            TestDatabase.UserId,
            new ClaimOpenState(),
            true,
            0m,
            now);

        return component.SubmitHistory(input).Value;
    }

    public static void MarkPosted(ReserveComponent component, ReserveHistory history)
    {
        var input = new HistoryPostingInput(history.Id) { JobId = "job-1" };
        component.MarkHistoryPosted(input);
    }
}
