using ClaimsModule.Application.Claims.Parties;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/parties")]
public sealed class ClaimPartiesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    [ProducesResponseType<CommandResponse<PartyDto>>(StatusCodes.Status201Created)]
    public Task<IActionResult> Add(
        Guid claimId,
        [FromBody] PartyRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(new AddPartyCommand(claimId, request), cancellationToken);
    }

    [HttpDelete("{partyId:guid}")]
    [ProducesResponseType<CommandResponse<PartyDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Remove(Guid claimId, Guid partyId, CancellationToken cancellationToken)
    {
        return CommandAsync(new RemovePartyCommand(claimId, partyId), cancellationToken);
    }
}
