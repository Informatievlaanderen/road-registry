namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.Text;
using Abstractions;
using Abstractions.Extracts;
using Azure.Core;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;

public class DownloadExtractByFileRequestHandler : EndpointRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    private readonly Encoding _encoding;

    public DownloadExtractByFileRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _encoding = Encoding.GetEncoding(1252);
    }

    public override async Task<DownloadExtractByFileResponse> HandleAsync(DownloadExtractByFileRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");

		//TODO-rik validate PRJ in validator
        //using (var reader = new StreamReader(stream, _encoding))
        //{
        //    var projectionFormat = ProjectionFormat.Read(reader);
        //    if (!projectionFormat.IsBelgeLambert1972()) problems += entry.ProjectionFormatInvalid();
        //}


        var contour = await GetRoadNetworkExtractGeometryFromShapeAsync(request.ShpFile, request.Buffer, cancellationToken);

        var message = new Command(
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = contour,
                DownloadId = downloadId,
                Description = request.Description
            });
        await Dispatcher(message, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId);
    }

    private async Task<RoadNetworkExtractGeometry> GetRoadNetworkExtractGeometryFromShapeAsync(DownloadExtractByFileRequestItem shapeFile, int buffer, CancellationToken cancellationToken)
    {
        using (var reader = new BinaryReader(shapeFile.ReadStream, _encoding))
        {
            ShapeFileHeader header = null;
            try
            {
                header = ShapeFileHeader.Read(reader);
            }
            catch (Exception exception)
            {
                //problems += entry.HasShapeHeaderFormatError(exception);
            }

            var polygons = new List<NetTopologySuite.Geometries.Polygon>();

            if (header != null)
                using (var records = header.CreateShapeRecordEnumerator(reader))
                {
                    while(records.MoveNext() && records.Current != null)
                    {
                        var shape = records.Current.Content;
                        //TODO-rik hoe komt een MPolygon hier binnen?
                        if (shape is PolygonShapeContent polygonShapeContent)
                        {
                            var polygon = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPolygon(polygonShapeContent.Shape);
                            polygons.Add(polygon);
                            continue;
                        }

                        throw new InvalidOperationException();

                        //var (recordProblems, recordContext) = _recordValidator.Validate(entry, records, context);
                        //problems += recordProblems;
                        //context = recordContext;
                    }
                }

            if (!polygons.Any())
            {
                //TODO-rik throw err
            }

            return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(new MultiPolygon(polygons.ToArray()), buffer);
        }
    }
}
