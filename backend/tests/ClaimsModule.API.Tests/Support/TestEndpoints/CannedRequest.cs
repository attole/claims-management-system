using ClaimsModule.Application.Results;
using MediatR;

namespace ClaimsModule.API.Tests.Support.TestEndpoints;

public sealed record CannedRequest<TData>(Result<TData> Result) : IRequest<Result<TData>>;
