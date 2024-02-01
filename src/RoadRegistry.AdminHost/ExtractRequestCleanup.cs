using System;
using System.Linq;
using System.Threading.Tasks;

namespace RoadRegistry.AdminHost
{
    using BackOffice;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Editor.Schema;
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using Infrastructure.Options;
    using Microsoft.Extensions.Logging;

    public class ExtractRequestCleanup
    {
        private readonly CommandHandlerDispatcher _dispatcher;
        private readonly EditorContext _editorContext;
        private readonly AdminHostOptions _adminHostOptions;
        private readonly ILogger<ExtractRequestCleanup> _logger;

        public ExtractRequestCleanup(
            CommandHandlerDispatcher commandHandlerDispatcher,
            EditorContext editorContext,
            AdminHostOptions adminHostOptions,
            ILogger<ExtractRequestCleanup> logger
        )
        {
            _dispatcher = commandHandlerDispatcher;
            _editorContext = editorContext;
            _adminHostOptions = adminHostOptions;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            do
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"An unhandled exception has occurred: {ex.Message}");
                }
                finally
                {
                    if (_adminHostOptions.AlwaysRunning && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(new TimeSpan(1, 0, 0), cancellationToken);
                    }
                }
            } while (_adminHostOptions.AlwaysRunning && !cancellationToken.IsCancellationRequested);
        }
    }
}
