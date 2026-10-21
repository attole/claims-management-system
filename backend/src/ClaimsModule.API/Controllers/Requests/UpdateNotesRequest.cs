using ClaimsModule.Application.Claims.Commands;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record UpdateNotesRequest(string Notes)
{
    public UpdateNotesCommand ToCommand(Guid claimId)
    {
        return new UpdateNotesCommand(claimId, Notes ?? string.Empty);
    }
}
