namespace RoadRegistry.Jobs.Processor
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
        private readonly ILogger _logger;

        public ExtractRequestCleaner(
            CommandHandlerDispatcher commandHandlerDispatcher,
            EditorContext editorContext,
            ILoggerFactory loggerFactory
        )
        {
            _dispatcher = commandHandlerDispatcher;
            _editorContext = editorContext;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task CloseOldExtracts(CancellationToken cancellationToken)
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
    }
}
