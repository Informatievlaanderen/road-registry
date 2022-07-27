namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Contracts.Extracts;
using Editor.Schema;
using Exceptions;
using Framework;
using MediatR.Pipeline;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

internal class DownloadExtractByNisCodeRequestHandler : EndpointRequestHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse>,
    IRequestExceptionHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse, DownloadExtractByNisCodeNotFoundException>
{
    private readonly EditorContext _context;

    public DownloadExtractByNisCodeRequestHandler(CommandHandlerDispatcher dispatcher, EditorContext context, ILogger<DownloadExtractByNisCodeRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    public Task Handle(
        DownloadExtractByNisCodeRequest request,
        DownloadExtractByNisCodeNotFoundException exception,
        RequestExceptionHandlerState<DownloadExtractByNisCodeResponse> state,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Could not download the extract presented by this contour");
        return Task.FromException(exception);
    }

    public override async Task<DownloadExtractByNisCodeResponse> HandleAsync(DownloadExtractByNisCodeRequest request, CancellationToken cancellationToken)
    {
        var municipalityGeometry = await _context.MunicipalityGeometries.SingleAsync(x => x.NisCode == request.NisCode, cancellationToken);

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
