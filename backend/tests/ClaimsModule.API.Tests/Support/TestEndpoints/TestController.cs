using System.ComponentModel.DataAnnotations;
using ClaimsModule.API.Controllers;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;
using ClaimsModule.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.API.Tests.Support.TestEndpoints;

[ApiController]
[Route("test")]
public sealed class TestController(
    ISender sender,
    ICurrentUser user,
    ICorrelationContext correlation,
    ClaimsDbContext context,
    IUnitOfWork unitOfWork) : ApiControllerBase(sender)
{
    private static int _echoCalls;

    public static int EchoCalls => Volatile.Read(ref _echoCalls);

    [HttpGet("whoami")]
    public ActionResult<WhoAmIDto> WhoAmI()
    {
        return Ok(new WhoAmIDto(user.Id, user.OrganisationId, user.Role.Marker.ToString(), correlation.CorrelationId));
    }

    [HttpGet("policies")]
    public async Task<ActionResult<int>> CountPolicies(CancellationToken cancellationToken)
    {
        int count = await context.Policies.CountAsync(cancellationToken);

        return Ok(count);
    }

    [AllowAnonymous]
    [HttpGet("anonymous")]
    public IActionResult Anonymous()
    {
        return Ok();
    }

    [HttpGet("read")]
    public Task<IActionResult> Read(CancellationToken cancellationToken)
    {
        return QueryAsync(new CannedRequest<int>(Result<int>.Success(7)), cancellationToken);
    }

    [HttpGet("paged")]
    public Task<IActionResult> Paged(CancellationToken cancellationToken)
    {
        var page = new PagedResult<string>(["a", "b"], 2, 25, 51);

        var request = new CannedRequest<PagedResult<string>>(Result<PagedResult<string>>.Success(page));

        return QueryAsync(request, cancellationToken);
    }

    [HttpGet("errors/{kind}")]
    public Task<IActionResult> Errors(ErrorKind kind, CancellationToken cancellationToken)
    {
        var errors = new List<Error>
        {
            new("TEST_ONE", "first message", kind, "Name"),
            new("TEST_TWO", "second message", kind, "Name"),
            new("TEST_THREE", "general message", kind),
        };

        return QueryAsync(new CannedRequest<int>(Result<int>.Failure(errors)), cancellationToken);
    }

    [ProducesResponseType<WriteEnvelope<int>>(StatusCodes.Status201Created)]
    [HttpPost("write-created")]
    public Task<IActionResult> WriteCreated(CancellationToken cancellationToken)
    {
        var warning = new Warning("TEST_WARNING", "be careful", "Notes");

        return CommandAsync(new CannedRequest<int>(Result<int>.Success(1, warning)), cancellationToken);
    }

    [ProducesResponseType<WriteEnvelope<int>>(StatusCodes.Status202Accepted)]
    [HttpPost("write-accepted")]
    public Task<IActionResult> WriteAccepted(CancellationToken cancellationToken)
    {
        return CommandAsync(new CannedRequest<int>(Result<int>.Success(1)), cancellationToken);
    }

    [HttpPost("write-plain")]
    public Task<IActionResult> WritePlain(CancellationToken cancellationToken)
    {
        return CommandAsync(new CannedRequest<int>(Result<int>.Success(1)), cancellationToken);
    }

    [ProducesResponseType<WriteEnvelope<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<WriteEnvelope<int>>(StatusCodes.Status201Created)]
    [HttpPost("write-ambiguous")]
    public Task<IActionResult> WriteAmbiguous(CancellationToken cancellationToken)
    {
        return CommandAsync(new CannedRequest<int>(Result<int>.Success(1)), cancellationToken);
    }

    [HttpPost("bind")]
    public IActionResult Bind(BindRequest request)
    {
        return Ok(request.Name);
    }

    [HttpGet("conflict")]
    public IActionResult ThrowConflict()
    {
        throw new ConcurrencyConflictException("test conflict", new InvalidOperationException("inner"));
    }

    [Authorize(Roles = "Nobody")]
    [HttpGet("forbidden")]
    public IActionResult Forbidden()
    {
        return Ok();
    }

    [HttpPost("echo")]
    public IActionResult Echo(EchoRequest request)
    {
        return Ok(new EchoResponse(Interlocked.Increment(ref _echoCalls), request.Text));
    }

    [HttpPost("echo-created")]
    public IActionResult EchoCreated(EchoRequest request)
    {
        return Created("/test/echo-created", new EchoResponse(Interlocked.Increment(ref _echoCalls), request.Text));
    }

    [HttpPost("echo-no-content")]
    public IActionResult EchoNoContent()
    {
        Interlocked.Increment(ref _echoCalls);

        return NoContent();
    }

    [HttpPost("echo-empty")]
    public IActionResult EchoEmpty()
    {
        Interlocked.Increment(ref _echoCalls);

        return Ok(null);
    }

    [HttpPost("echo-crash")]
    public async Task<IActionResult> EchoCrash(EchoRequest request, CancellationToken cancellationToken)
    {
        await unitOfWork.CommitAsync(cancellationToken);

        throw new InvalidOperationException($"crash {request.Text}");
    }

    [HttpPost("upload")]
    public IActionResult Upload(IFormFile file)
    {
        return Ok(new EchoResponse(Interlocked.Increment(ref _echoCalls), file.FileName));
    }

    [HttpPut("echo-put")]
    public IActionResult EchoPut(EchoRequest request)
    {
        return Ok(new EchoResponse(Interlocked.Increment(ref _echoCalls), request.Text));
    }

    [HttpGet("echo-get")]
    public IActionResult EchoGet()
    {
        return Ok(new EchoResponse(Interlocked.Increment(ref _echoCalls), "get"));
    }

    public sealed record BindRequest([Required] string Name);

    public sealed record EchoRequest(string Text);

    public sealed record EchoResponse(int Calls, string Text);
}
