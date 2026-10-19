using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.Inputs;

namespace ClaimsModule.Application.Tests.Support.ReserveComponents;

public static class ApplicationReserves
{
    public static readonly Guid Submitter = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly DateTimeOffset _now = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

    public static ReserveComponent Component(Claim claim)
    {
        var input = new OpenReserveComponentInput(claim.Reference, new ReserveIndemnityComponentType(), string.Empty);

        return ReserveComponent.Open(input).Value;
    }

    public static ReserveHistory Submit(ReserveComponent component, Claim claim, decimal amount, Guid actorId)
    {
        var input = new SubmitHistoryInput(
            ReserveTransactionType.Add,
            amount,
            "Initial estimate",
            actorId,
            claim.State,
            true,
            0m,
            _now);

        return component.SubmitHistory(input).Value;
    }

    public static ReserveHistory FailPosting(ReserveComponent component, ReserveHistory history)
    {
        var input = new HistoryPostingInput(history.Id)
        {
            JobId = "job-1",
            FailureReason = "Ledger unavailable",
        };
        component.MarkHistoryPostingFailed(input);

        return history;
    }
}
