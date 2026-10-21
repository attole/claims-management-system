using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Claims.Dtos;

public sealed record ClaimStatusCheckDto(IReadOnlyList<Error> Errors, IReadOnlyList<Warning> Warnings);
