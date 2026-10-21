using ClaimsModule.Application.Results;
using MediatR;

namespace ClaimsModule.API.Tests.Support.TestEndpoints;

public sealed class CannedRequestHandler<TData> : IRequestHandler<CannedRequest<TData>, Result<TData>>
{
    public Task<Result<TData>> Handle(CannedRequest<TData> request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Result);
    }
}
