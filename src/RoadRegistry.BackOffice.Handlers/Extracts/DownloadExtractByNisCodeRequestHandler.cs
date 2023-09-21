namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

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

        var message = new RequestRoadNetworkExtract
        {
            ExternalRequestId = randomExternalRequestId,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(municipalityGeometry.Geometry.ToMultiPolygon(), request.Buffer),
            DownloadId = downloadId,
            Description = request.Description,
            IsInformative = request.IsInformative
        };

        var command = new Command(message);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractByNisCodeResponse(downloadId, request.IsInformative);
    }
}
