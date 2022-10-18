namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

public class DownloadExtractByNisCodeRequestHandler : EndpointRequestHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse>
{
    private readonly EditorContext _context;

    public DownloadExtractByNisCodeRequestHandler(CommandHandlerDispatcher dispatcher, EditorContext context, ILogger<DownloadExtractByNisCodeRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public override async Task<DownloadExtractByNisCodeResponse> HandleAsync(DownloadExtractByNisCodeRequest request, CancellationToken cancellationToken)
    {
        var municipalityGeometry = await _context.MunicipalityGeometries.SingleOrDefaultAsync(x => x.NisCode == request.NisCode, cancellationToken)
                                   ?? throw new DownloadExtractByNisCodeNotFoundException("Could not find details about the supplied NIS code");

        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");

        var message = new Command(
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(
                    municipalityGeometry.Geometry as MultiPolygon, request.Buffer),
                DownloadId = downloadId,
                Description = request.Description
            });

        await Dispatcher(message, cancellationToken);

        return new DownloadExtractByNisCodeResponse(downloadId);
    }
}