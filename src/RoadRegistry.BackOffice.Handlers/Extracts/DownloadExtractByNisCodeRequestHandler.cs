namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Sync.MunicipalityRegistry;

public class DownloadExtractByNisCodeRequestHandler : ExtractRequestHandler<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse>
{
    private readonly MunicipalityEventConsumerContext _municipalityContext;
    private readonly UseExtractZipArchiveWriterV2FeatureToggle _useExtractZipArchiveWriterV2FeatureToggle;

    public DownloadExtractByNisCodeRequestHandler(
        MunicipalityEventConsumerContext municipalityContext,
        CommandHandlerDispatcher dispatcher,
        UseExtractZipArchiveWriterV2FeatureToggle useExtractZipArchiveWriterV2FeatureToggle,
        ILogger<DownloadExtractByNisCodeRequestHandler> logger) : base(dispatcher, logger)
    {
        _municipalityContext = municipalityContext;
        _useExtractZipArchiveWriterV2FeatureToggle = useExtractZipArchiveWriterV2FeatureToggle;
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
            IsInformative = request.IsInformative,
            ZipArchiveWriterVersion = _useExtractZipArchiveWriterV2FeatureToggle.FeatureEnabled
                ? WellKnownZipArchiveWriterVersions.V2
                : WellKnownZipArchiveWriterVersions.V1
        };

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractByNisCodeResponse(downloadId, request.IsInformative);
    }
}
