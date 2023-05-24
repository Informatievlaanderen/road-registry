namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Framework;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Reflection.PortableExecutable;

public class DownloadExtractByNisCodeRequestHandler : ExtractRequestHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse>
{
    public DownloadExtractByNisCodeRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByNisCodeRequestHandler> logger) : base(context, dispatcher, logger)
    {
    }

    public override async Task<DownloadExtractByNisCodeResponse> HandleRequestAsync(DownloadExtractByNisCodeRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var municipalityGeometry = await _context.MunicipalityGeometries.SingleOrDefaultAsync(x => x.NisCode == request.NisCode, cancellationToken)
                                   ?? throw new DownloadExtractByNisCodeNotFoundException("Could not find details about the supplied NIS code");

        await DispatchCommandWithContextAddAsync(
            new ExtractRequestRecord
            {
                RequestedOn = DateTime.UtcNow.ToFileTimeUtc(),
                ExternalRequestId = randomExternalRequestId,
                Contour = municipalityGeometry.Geometry,
                DownloadId = downloadId,
                Description = request.Description,
                UploadExpected = request.UploadExpected
            },
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(
                    municipalityGeometry.Geometry as MultiPolygon, request.Buffer),
                DownloadId = downloadId,
                Description = request.Description,
                UploadExpected = request.UploadExpected
            }, cancellationToken);

        return new DownloadExtractByNisCodeResponse(downloadId, request.UploadExpected);
    }
}
