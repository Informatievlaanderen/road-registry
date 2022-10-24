namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.Text;
using Abstractions;
using Abstractions.Extracts;
using Azure.Core;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;

public class DownloadExtractByFileRequestHandler : EndpointRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    public DownloadExtractByFileRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
    }

    public override async Task<DownloadExtractByFileResponse> HandleAsync(DownloadExtractByFileRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");
        var contour = await GetRoadNetworkExtractGeometryFromShapeAsync(request.ShpFile, request.Buffer, cancellationToken);

        var message = new Command(
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = contour,
                DownloadId = downloadId,
                Description = request.Description
            });
        await Dispatcher(message, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId);
    }

    private static async Task<RoadNetworkExtractGeometry> GetRoadNetworkExtractGeometryFromShapeAsync(DownloadExtractByFileRequestItem shapeFile, int buffer, CancellationToken cancellationToken)
    {
        using var reader = new BinaryReader(shapeFile.ReadStream, Encoding.UTF8);
        var shapeContent = ShapeContent.Read(reader);

        var result = await Task.FromResult(GeometryTranslator.TranslateToRoadNetworkExtractGeometry(shapeContent as IPolygonal, buffer));
        return result;
    }
}
