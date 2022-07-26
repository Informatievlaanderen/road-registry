namespace RoadRegistry.Api.BackOffice.Abstractions
{
    using MediatR;
    using RoadRegistry.BackOffice.Framework;
    using System.Threading.Tasks;
    using FluentValidation;

    public abstract class EndpointRequestHandler<TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        protected EndpointRequestHandler()
        {
        }

        public async Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
        {
            await HandleAsync(request, cancellationToken);
            return Unit.Value;
        }

        public abstract Task HandleAsync(TRequest request, CancellationToken cancellationToken);
    }

    public abstract class EndpointRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : EndpointRequest<TResponse>
        where TResponse : EndpointResponse
    {
        protected CommandHandlerDispatcher Dispatcher { get; init; }
        protected IValidator<TRequest> Validator { get; init; }

        protected EndpointRequestHandler(CommandHandlerDispatcher dispatcher, IValidator<TRequest> validator)
        {
            Dispatcher = dispatcher;
            Validator = validator;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var response = await HandleAsync(request, cancellationToken);
            return response;
        }

        public abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
