using ClaimsModule.Application.ReserveComponents.History.Commands;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record RejectReserveRequest(string RejectionReason)
{
    public RejectReserveCommand ToCommand(Guid claimId, Guid reserveHistoryId)
    {
        return new RejectReserveCommand(claimId, reserveHistoryId, RejectionReason ?? string.Empty);
    }
}
