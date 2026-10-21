using ClaimsModule.Application.Claims.RiskObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/risk-objects")]
public sealed class ClaimRiskObjectsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    [ProducesResponseType<CommandResponse<RiskObjectDto>>(StatusCodes.Status201Created)]
    public Task<IActionResult> Add(
        Guid claimId,
        [FromBody] RiskObjectRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(new AddRiskObjectCommand(claimId, request), cancellationToken);
    }

    [HttpDelete("{riskObjectId:guid}")]
    [ProducesResponseType<CommandResponse<RiskObjectDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Remove(Guid claimId, Guid riskObjectId, CancellationToken cancellationToken)
    {
        return CommandAsync(new RemoveRiskObjectCommand(claimId, riskObjectId), cancellationToken);
    }
}
