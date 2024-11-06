namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class DownloadExtractByContourRequestHandler : ExtractRequestHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse>
{
    private readonly WKTReader _reader;

    public DownloadExtractByContourRequestHandler(
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    protected override async Task<DownloadExtractByContourResponse> HandleRequestAsync(DownloadExtractByContourRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var geometry = _reader.Read(request.Contour).ToMultiPolygon();

        var message = new RequestRoadNetworkExtract
        {
            ExternalRequestId = randomExternalRequestId,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(geometry, request.Buffer),
            DownloadId = downloadId,
            Description = request.Description,
            IsInformative = request.IsInformative
        };

        var command = new Command(message);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractByContourResponse(downloadId, request.IsInformative);
    }
}
