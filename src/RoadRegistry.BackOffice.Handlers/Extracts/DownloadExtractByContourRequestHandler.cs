namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Contracts.Extracts;
using Exceptions;
using Framework;
using MediatR.Pipeline;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

internal class DownloadExtractByContourRequestHandler : EndpointRequestHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse>,
    IRequestExceptionHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse, DownloadExtractByContourNotFoundException>
{
    private readonly WKTReader _reader;

    public DownloadExtractByContourRequestHandler(CommandHandlerDispatcher dispatcher, WKTReader reader, ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public Task Handle(
        DownloadExtractByContourRequest request,
        DownloadExtractByContourNotFoundException exception,
        RequestExceptionHandlerState<DownloadExtractByContourResponse> state,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Could not download the extract presented by this contour");
        return Task.FromException(exception);
    }

    public override async Task<DownloadExtractByContourResponse> HandleAsync(DownloadExtractByContourRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");
        var message = new Command(
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(_reader.Read(request.Contour) as IPolygonal, request.Buffer),
                DownloadId = downloadId,
                Description = request.Description
            });
        await Dispatcher(message, cancellationToken);

        return new DownloadExtractByContourResponse(downloadId);
    }
}
