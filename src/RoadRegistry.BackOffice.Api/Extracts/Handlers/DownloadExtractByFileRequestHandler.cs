namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using BackOffice.Handlers.Extracts;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class DownloadExtractByFileRequestHandler : ExtractRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    private readonly IDownloadExtractByFileRequestItemTranslator _translator;
    private readonly UseExtractZipArchiveWriterV2FeatureToggle _useExtractZipArchiveWriterV2FeatureToggle;

    public DownloadExtractByFileRequestHandler(
        CommandHandlerDispatcher dispatcher,
        IDownloadExtractByFileRequestItemTranslator translator,
        UseExtractZipArchiveWriterV2FeatureToggle useExtractZipArchiveWriterV2FeatureToggle,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _translator = translator;
        _useExtractZipArchiveWriterV2FeatureToggle = useExtractZipArchiveWriterV2FeatureToggle;
    }

    protected override async Task<DownloadExtractByFileResponse> HandleRequestAsync(DownloadExtractByFileRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var contour = _translator.Translate(request.ShpFile, request.Buffer);

        var message = new RequestRoadNetworkExtract
        {
            ExternalRequestId = randomExternalRequestId,
            Contour = contour,
            DownloadId = downloadId,
            Description = request.Description,
            IsInformative = request.IsInformative,
            ZipArchiveWriterVersion = _useExtractZipArchiveWriterV2FeatureToggle.FeatureEnabled
                ? WellKnownZipArchiveWriterVersions.V2
                : WellKnownZipArchiveWriterVersions.V1
        };

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId, request.IsInformative);
    }
}
