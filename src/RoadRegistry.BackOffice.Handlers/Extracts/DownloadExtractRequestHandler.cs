namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extracts;

public class DownloadExtractRequestHandler : ExtractRequestHandler<DownloadExtractRequest, DownloadExtractResponse>
{
    private readonly WKTReader _reader;
    private readonly UseGrbExtractZipArchiveWriterV2FeatureToggle _useGrbExtractZipArchiveWriterV2FeatureToggle;

    public DownloadExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        UseGrbExtractZipArchiveWriterV2FeatureToggle useGrbExtractZipArchiveWriterV2FeatureToggle,
        ILogger<DownloadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _useGrbExtractZipArchiveWriterV2FeatureToggle = useGrbExtractZipArchiveWriterV2FeatureToggle;
    }

    protected override async Task<DownloadExtractResponse> HandleRequestAsync(DownloadExtractRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var message = new RequestRoadNetworkExtract
        {
            ExternalRequestId = request.RequestId,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry((IPolygonal)_reader.Read(request.Contour)),
            DownloadId = downloadId,
            Description = request.RequestId[..(request.RequestId.Length > ExtractDescription.MaxLength ? ExtractDescription.MaxLength : request.RequestId.Length)],
            IsInformative = request.IsInformative,
            ZipArchiveWriterVersion = _useGrbExtractZipArchiveWriterV2FeatureToggle.FeatureEnabled
                ? WellKnownZipArchiveWriterVersions.V2
                : WellKnownZipArchiveWriterVersions.V1
        };

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractResponse(downloadId, request.IsInformative);
    }
}
