using RoadRegistry.BackOffice.Framework;

namespace RoadRegistry.Api.BackOffice.Handlers
{
    internal class DownloadProductRequestHandler : EndpointRequestHandler<DownloadProductRequest, DownloadProductResponse>,
        IRequestExceptionHandler<DownloadProductRequest, DownloadProductResponse, DownloadProductNotFoundException>
    {
        public DownloadProductRequestHandler(CommandHandlerDispatcher dispatcher) : base(dispatcher)
        {
        }

        public Task Handle(
            DownloadProductRequest request,
            DownloadProductNotFoundException exception,
            RequestExceptionHandlerState<DownloadProductResponse> state,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<DownloadProductResponse> HandleAsync(DownloadProductRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}