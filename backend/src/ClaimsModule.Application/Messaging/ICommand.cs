using ClaimsModule.Application.Results;
using MediatR;

namespace ClaimsModule.Application.Messaging;

public interface ICommand<TResponse> : IRequest<TResponse>
    where TResponse : IFailableResult<TResponse>;
