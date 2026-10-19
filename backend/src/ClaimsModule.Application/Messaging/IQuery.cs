using ClaimsModule.Application.Results;
using MediatR;

namespace ClaimsModule.Application.Messaging;

public interface IQuery<TResponse> : IRequest<TResponse>
    where TResponse : IFailableResult<TResponse>;
