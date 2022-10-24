namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Framework;
using Microsoft.Extensions.Logging;

public class DownloadExtractByFileRequestHandler : EndpointRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    public DownloadExtractByFileRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
    }

    public override async Task<DownloadExtractByFileResponse> HandleAsync(DownloadExtractByFileRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");

        //var message = new Command(
        //    new RequestRoadNetworkExtract
        //    {
        //        ExternalRequestId = randomExternalRequestId,
        //        Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(_reader.Read(request.Contour) as IPolygonal, request.Buffer),
        //        DownloadId = downloadId,
        //        Description = request.Description
        //    });
        //await Dispatcher(message, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId);
    }
}
