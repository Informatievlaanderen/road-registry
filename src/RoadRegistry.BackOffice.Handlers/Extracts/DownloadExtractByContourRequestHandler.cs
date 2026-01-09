namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;

public class DownloadExtractByContourRequestHandler : ExtractRequestHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse>
{
    private readonly WKTReader _reader;
    private readonly UseExtractZipArchiveWriterV2FeatureToggle _useExtractZipArchiveWriterV2FeatureToggle;

    public DownloadExtractByContourRequestHandler(
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        UseExtractZipArchiveWriterV2FeatureToggle useExtractZipArchiveWriterV2FeatureToggle,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _useExtractZipArchiveWriterV2FeatureToggle = useExtractZipArchiveWriterV2FeatureToggle;
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
            IsInformative = request.IsInformative,
            ZipArchiveWriterVersion = _useExtractZipArchiveWriterV2FeatureToggle.FeatureEnabled
                ? WellKnownZipArchiveWriterVersions.V2
                : WellKnownZipArchiveWriterVersions.V1
        };

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractByContourResponse(downloadId, request.IsInformative);
    }
}
