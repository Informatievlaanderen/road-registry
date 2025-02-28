namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Sync.MunicipalityRegistry;

public class DownloadExtractByNisCodeRequestHandler : ExtractRequestHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse>
{
    private readonly MunicipalityEventConsumerContext _municipalityContext;

    public DownloadExtractByNisCodeRequestHandler(
        MunicipalityEventConsumerContext municipalityContext,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByNisCodeRequestHandler> logger) : base(dispatcher, logger)
    {
        _municipalityContext = municipalityContext;
    }

    protected override async Task<DownloadExtractByNisCodeResponse> HandleRequestAsync(DownloadExtractByNisCodeRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var municipality = await _municipalityContext.FindCurrentMunicipalityByNisCode(request.NisCode, cancellationToken);
        if (municipality?.Geometry is null)
        {
            throw new DownloadExtractByNisCodeNotFoundException("Could not find details about the supplied NIS code");
        }

        var message = new RequestRoadNetworkExtract
        {
            ExternalRequestId = randomExternalRequestId,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(municipality.Geometry.ToMultiPolygon(), request.Buffer),
            DownloadId = downloadId,
            Description = request.Description,
            IsInformative = request.IsInformative
        };

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractByNisCodeResponse(downloadId, request.IsInformative);
    }
}
