namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class DownloadExtractRequestHandler : EndpointRequestHandler<DownloadExtractRequest, DownloadExtractResponse>
{
    private readonly WKTReader _reader;

    public DownloadExtractRequestHandler(CommandHandlerDispatcher dispatcher, WKTReader reader, ILogger<DownloadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
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
                Description = request.RequestId
            });

        await Dispatcher(message, cancellationToken);

        return new DownloadExtractResponse(downloadId);
    }
}
