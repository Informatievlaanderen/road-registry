namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Contracts.Extracts;
using Exceptions;
using Framework;
using MediatR.Pipeline;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

internal class DownloadExtractRequestHandler : EndpointRequestHandler<DownloadExtractRequest, DownloadExtractResponse>,
    IRequestExceptionHandler<DownloadExtractRequest, DownloadExtractResponse, DownloadExtractNotFoundException>
{
    private readonly WKTReader _reader;

    public DownloadExtractRequestHandler(CommandHandlerDispatcher dispatcher, WKTReader reader, ILogger<DownloadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public Task Handle(
        DownloadExtractRequest request,
        DownloadExtractNotFoundException exception,
        RequestExceptionHandlerState<DownloadExtractResponse> state,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<DownloadExtractResponse> HandleAsync(DownloadExtractRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var message = new Command(
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = request.RequestId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry((IPolygonal)_reader.Read(request.Contour)),
                DownloadId = downloadId,
                Description = string.Empty
            });

        await Dispatcher(message, cancellationToken);
        return new DownloadExtractResponse(downloadId);
    }
}
