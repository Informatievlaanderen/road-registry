namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.Text;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class DownloadExtractByFileRequestItemTranslator
{
    private readonly Encoding _encoding;
    private readonly ILogger<DownloadExtractByFileRequestItemTranslator> _logger;

    public DownloadExtractByFileRequestItemTranslator(Encoding encoding, ILogger<DownloadExtractByFileRequestItemTranslator> logger)
    {
        _encoding = encoding;
        _logger = logger;
    }

    public RoadNetworkExtractGeometry Translate(DownloadExtractByFileRequestItem shapeFile, int buffer)
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

                    if (polygons.Any())
                    {
                        var srids = polygons.Select(x => x.SRID).Distinct().ToArray();
                        if (srids.Length > 1)
                        {
                            AddValidationFailure("All geometries must have the same SRID.");
                        }
                    }
                    else
                    {
                        AddValidationFailure("Invalid shape file. Does not contain any valid polygon geometries.");
                    }
                }
                else
                {
                    AddValidationFailure("Geometry type must be polygon or multipolygon");
                }
            }

            if (validationFailures.Any()) throw new ValidationException("Shape file content contains some errors", validationFailures);

            var srid = polygons.First().SRID;
            return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(new MultiPolygon(polygons.ToArray()) { SRID = srid }, buffer);
        }

        void AddValidationFailure(string errorMessage)
        {
            var vf = new ValidationFailure(nameof(DownloadExtractByFileRequest.ShpFile), errorMessage);
            validationFailures.Add(vf);
            _logger.LogWarning("Added validation failure while processing current shape file record: '{ValidationFailureMessage}'", vf.ErrorMessage);
        }
    }
}
