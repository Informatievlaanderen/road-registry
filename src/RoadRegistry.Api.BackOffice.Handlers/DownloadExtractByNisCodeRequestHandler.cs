namespace RoadRegistry.Api.BackOffice.Handlers;

using RoadRegistry.BackOffice.Framework;

internal class DownloadExtractByNisCodeRequestHandler : EndpointRequestHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse>,
    IRequestExceptionHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse, DownloadExtractByNisCodeNotFoundException>
{
    public DownloadExtractByNisCodeRequestHandler(CommandHandlerDispatcher dispatcher) : base(dispatcher)
    {
    }

    public Task Handle(
        DownloadExtractByNisCodeRequest request,
        DownloadExtractByNisCodeNotFoundException exception,
        RequestExceptionHandlerState<DownloadExtractByNisCodeResponse> state,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<DownloadExtractByNisCodeResponse> HandleAsync(DownloadExtractByNisCodeRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
