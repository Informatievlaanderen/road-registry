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

    public class ExtractRequestCleanup
    {
        private readonly CommandHandlerDispatcher _dispatcher;
        private readonly EditorContext _editorContext;

        public ExtractRequestCleanup(
            CommandHandlerDispatcher commandHandlerDispatcher,
            EditorContext editorContext)
        {
            _dispatcher = commandHandlerDispatcher;
            _editorContext = editorContext;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var extractRequests = await _editorContext.ExtractRequests
                .Where(extractRequest =>
                    !extractRequest.IsInformative &&
                    extractRequest.RequestedOn.Date <= DateTimeOffset.Now.Date.AddDays(-7) &&
                    extractRequest.DownloadedOn == null)
                .ToListAsync(cancellationToken);
            
            foreach (var extractRequest in extractRequests)
            {
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
