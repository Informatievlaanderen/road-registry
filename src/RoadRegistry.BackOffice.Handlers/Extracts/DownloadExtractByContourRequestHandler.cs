namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class DownloadExtractByContourRequestHandler : ExtractRequestHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse>
{
    private readonly WKTReader _reader;

    public DownloadExtractByContourRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(context, dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public override async Task<DownloadExtractByContourResponse> HandleRequestAsync(DownloadExtractByContourRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var geometry = _reader.Read(request.Contour);

        await DispatchCommandWithContextAddAsync(
            new ExtractRequestRecord
            {
                DownloadId = downloadId,
                Contour = geometry,
                Description = request.Description,
                UploadExpected = request.UploadExpected
            },
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(geometry as IPolygonal, request.Buffer),
                DownloadId = downloadId,
                Description = request.Description,
                UploadExpected = request.UploadExpected
            }, cancellationToken);

        return new DownloadExtractByContourResponse(downloadId, request.UploadExpected);
    }
}
