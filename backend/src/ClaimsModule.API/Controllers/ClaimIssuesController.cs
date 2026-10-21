using ClaimsModule.Application.Claims.Issues;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/issues/{issueId:guid}")]
public sealed class ClaimIssuesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("acknowledge")]
    [ProducesResponseType<CommandResponse<IssueDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Acknowledge(Guid claimId, Guid issueId, CancellationToken cancellationToken)
    {
        return CommandAsync(new AcknowledgeIssueCommand(claimId, issueId), cancellationToken);
    }

    [HttpPost("waive")]
    [ProducesResponseType<CommandResponse<IssueDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Waive(Guid claimId, Guid issueId, CancellationToken cancellationToken)
    {
        return CommandAsync(new WaiveIssueCommand(claimId, issueId), cancellationToken);
    }
}
