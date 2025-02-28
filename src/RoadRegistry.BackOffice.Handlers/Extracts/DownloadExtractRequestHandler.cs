namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class DownloadExtractRequestHandler : ExtractRequestHandler<DownloadExtractRequest, DownloadExtractResponse>
{
    private readonly WKTReader _reader;

    public DownloadExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        ILogger<DownloadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    protected override async Task<DownloadExtractResponse> HandleRequestAsync(DownloadExtractRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var message = new RequestRoadNetworkExtract
        {
            ExternalRequestId = request.RequestId,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry((IPolygonal)_reader.Read(request.Contour)),
            DownloadId = downloadId,
            Description = request.RequestId[..(request.RequestId.Length > ExtractDescription.MaxLength ? ExtractDescription.MaxLength : request.RequestId.Length)],
            IsInformative = request.IsInformative
        };

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractResponse(downloadId, request.IsInformative);
    }
}
