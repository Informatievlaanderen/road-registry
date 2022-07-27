namespace RoadRegistry.Api.BackOffice.Handlers;

using Microsoft.AspNetCore.Http;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

internal class DownloadExtractByContourRequestHandler : EndpointRequestHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse>,
    IRequestExceptionHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse, DownloadExtractByContourNotFoundException>
{
    private readonly WKTReader _reader;

    public DownloadExtractByContourRequestHandler(CommandHandlerDispatcher dispatcher, WKTReader reader ) : base(dispatcher)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public Task Handle(
        DownloadExtractByContourRequest request,
        DownloadExtractByContourNotFoundException exception,
        RequestExceptionHandlerState<DownloadExtractByContourResponse> state,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<DownloadExtractByContourResponse> HandleAsync(DownloadExtractByContourRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");
        var message = new Command(
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(_reader.Read(request.Contour) as IPolygonal, request.Buffer),
                DownloadId = downloadId,
                Description = request.Description
            });
        await Dispatcher(message, cancellationToken);

        return new DownloadExtractByContourResponse(downloadId);
    }
}
