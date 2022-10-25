namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.Text;
using Abstractions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class DownloadExtractByFileRequestHandler : EndpointRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    private readonly Encoding _encoding;

    public DownloadExtractByFileRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _encoding = WellKnownEncodings.WindowsAnsi;
    }

    public override async Task<DownloadExtractByFileResponse> HandleAsync(DownloadExtractByFileRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");

        var contour = HandleRoadNetworkExtractGeometryFromShape(request.ShpFile, request.Buffer);

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

    private RoadNetworkExtractGeometry HandleRoadNetworkExtractGeometryFromShape(DownloadExtractByFileRequestItem shapeFile, int buffer)
    {
        var validationFailures = Enumerable.Empty<ValidationFailure>().ToList();

        using (var reader = new BinaryReader(shapeFile.ReadStream, _encoding))
        {
            ShapeFileHeader header = null;

            var polygons = new List<Polygon>();

            try
            {
                header = ShapeFileHeader.Read(reader);
            }
            catch (Exception exception)
            {
                AddValidationFailure($"Could not read header from shape file: '{exception.Message}'");
            }

            if (header is not null)
            {
                if (new[] { ShapeType.Polygon, ShapeType.PolygonM }.Contains(header.ShapeType))
                {
                    using (var records = header.CreateShapeRecordEnumerator(reader))
                    {
                        while (records.MoveNext() && records.Current != null)
                            switch (records.Current.Content)
                            {
                                case PolygonShapeContent polygonShapeContent:
                                    polygons.AddRange(GeometryTranslator.ToGeometryMultiPolygon(polygonShapeContent.Shape).Geometries.Cast<Polygon>());

                                    break;

                                default:
                                    AddValidationFailure("Geometry type must be polygon or multipolygon");

                                    break;
                            }
                    }

                    if (!polygons.Any()) AddValidationFailure("Invalid shape file. Does not contain any valid polygon geometries.");
                }
                else
                {
                    AddValidationFailure("Geometry type must be polygon or multipolygon");
                }
            }

            if (validationFailures.Any()) throw new ValidationException("Shape file content contains some errors", validationFailures);

            return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(new MultiPolygon(polygons.ToArray()), buffer);
        }

        void AddValidationFailure(string errorMessage)
        {
            var vf = new ValidationFailure(nameof(DownloadExtractByFileRequest.ShpFile), errorMessage);
            validationFailures.Add(vf);
            _logger.LogWarning("Added validation failure while processing current shape file record: '{ValidationFailureMessage}'", vf.ErrorMessage);
        }
    }
}
