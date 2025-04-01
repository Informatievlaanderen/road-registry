namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using BackOffice.Extensions;
using Core;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using ShapeFile;
using GeometryTranslator = BackOffice.GeometryTranslator;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Problem = Core.Problem;

public interface IDownloadExtractByFileRequestItemTranslator
{
    RoadNetworkExtractGeometry Translate(DownloadExtractByFileRequestItem shapeFile, int buffer);
}

public class DownloadExtractByFileRequestItemTranslator : IDownloadExtractByFileRequestItemTranslator
{
    public RoadNetworkExtractGeometry Translate(DownloadExtractByFileRequestItem shapeFile, int buffer)
    {
        var problems = new List<Problem>();

        ShapeType? shapeType = null;
        Geometry geometry = null;
        try
        {
            (shapeType, geometry) = new ExtractGeometryShapeFileReader().Read(shapeFile.ReadStream);
        }
        catch (Exception ex)
        {
            problems.Add(new ShapeFileInvalidHeader(ex));
        }

        var polygons = new List<Polygon>();

        if (shapeType is not null)
        {
            if (new[] { ShapeType.Polygon, ShapeType.PolygonM, ShapeType.PolygonZM }.Contains(shapeType.Value))
            {
                if (geometry is Polygon || geometry is MultiPolygon)
                {
                    polygons.AddRange(geometry.ToMultiPolygon().Cast<Polygon>());
                }
                else
                {
                    problems.Add(new ShapeFileGeometryTypeMustBePolygon());
                }

                if (polygons.Any())
                {
                    var srids = polygons.Select(x => x.SRID).Distinct().ToArray();
                    if (srids.Length > 1)
                    {
                        problems.Add(new ShapeFileGeometrySridMustBeEqual());
                    }
                }
                else
                {
                    problems.Add(new ShapeFileHasNoValidPolygons());
                }
            }
            else
            {
                problems.Add(new ShapeFileGeometryTypeMustBePolygon());
            }
        }

        if (problems.Any())
        {
            throw new ValidationException("Shape file content contains some errors",
                problems
                    .Select(problem => problem.TranslateToDutch())
                    .Select(problemTranslation => new ValidationFailure
                    {
                        PropertyName = nameof(DownloadExtractByFileRequest.ShpFile),
                        ErrorMessage = problemTranslation.Message,
                        ErrorCode = problemTranslation.Code
                    }));
        }

        var srid = polygons.First().SRID;
        return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(new MultiPolygon(polygons.ToArray()) { SRID = srid }, buffer);
    }
}
