namespace RoadRegistry.Jobs.Processor
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Extracts.Schema;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Framework;
    using RoadRegistry.BackOffice.Messages;
    using RoadRegistry.Editor.Schema;

    public interface IExtractRequestCleaner
    {
        Task CloseOldExtracts(CancellationToken cancellationToken);
    }

    public class ExtractRequestCleaner : IExtractRequestCleaner
    {
        private readonly CommandHandlerDispatcher _dispatcher;
        private readonly EditorContext _editorContext;
        private readonly ExtractsDbContext _extractsDbContext;
        private readonly ILogger _logger;

        public ExtractRequestCleaner(
            CommandHandlerDispatcher commandHandlerDispatcher,
            EditorContext editorContext,
            ExtractsDbContext extractsDbContext,
            ILoggerFactory loggerFactory
        )
        {
            _dispatcher = commandHandlerDispatcher;
            _editorContext = editorContext;
            _extractsDbContext = extractsDbContext;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task CloseOldExtracts(CancellationToken cancellationToken)
        {
            await CloseOldExtractsV1(cancellationToken);
            await CloseOldExtractsV2(cancellationToken);
        }

        private async Task CloseOldExtractsV1(CancellationToken cancellationToken)
        {
            var extractRequests = await _editorContext.ExtractRequests
                .Where(extractRequest =>
                    !extractRequest.IsInformative &&
                    (
                        extractRequest.RequestedOn.Date <= DateTimeOffset.Now.Date.AddMonths(-6)
                        ||
                        (extractRequest.DownloadedOn == null && extractRequest.RequestedOn.Date <= DateTimeOffset.Now.Date.AddDays(-7))
                    )
                )
                .ToListAsync(cancellationToken);

            foreach (var extractRequest in extractRequests)
            {
                _logger.LogInformation("Closing extract with DownloadId {DownloadId}, ExternalRequestId: {ExternalRequestId}", extractRequest.DownloadId, extractRequest.ExternalRequestId);

                var message = new CloseRoadNetworkExtract
                {
                    ExternalRequestId = new ExternalExtractRequestId(extractRequest.ExternalRequestId),
                    Reason = RoadNetworkExtractCloseReason.NoDownloadReceived,
                    DownloadId = new DownloadId(extractRequest.DownloadId)
                };
                var command = new Command(message);
                await _dispatcher(command, cancellationToken);
            }
        }

        private async Task CloseOldExtractsV2(CancellationToken cancellationToken)
        {
            var extractDownloads = await _extractsDbContext.ExtractDownloads
                .Where(extractDownload =>
                    !extractDownload.IsInformative &&
                    (
                        extractDownload.RequestedOn.Date <= DateTimeOffset.Now.Date.AddMonths(-6)
                        ||
                        (extractDownload.DownloadedOn == null && extractDownload.RequestedOn.Date <= DateTimeOffset.Now.Date.AddDays(-7))
                    )
                )
                .ToListAsync(cancellationToken);

            foreach (var extractDownload in extractDownloads)
            {
                _logger.LogInformation("Closing extract with DownloadId {DownloadId}", extractDownload.DownloadId);

                extractDownload.Closed = true;
            }

            await _extractsDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
