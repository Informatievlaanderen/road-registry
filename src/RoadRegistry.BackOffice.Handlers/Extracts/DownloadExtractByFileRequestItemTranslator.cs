namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;
using System.Text;
using GeometryTranslator = BackOffice.GeometryTranslator;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Problem = Core.Problem;

public interface IDownloadExtractByFileRequestItemTranslator
{
    RoadNetworkExtractGeometry Translate(DownloadExtractByFileRequestItem shapeFile, int buffer);
}

[Obsolete("Use DownloadExtractByFileRequestItemTranslatorNetTopologySuite instead")]
public class DownloadExtractByFileRequestItemTranslator : IDownloadExtractByFileRequestItemTranslator
{
    private readonly Encoding _encoding;

    public DownloadExtractByFileRequestItemTranslator(Encoding encoding)
    {
        _encoding = encoding;
    }

    public RoadNetworkExtractGeometry Translate(DownloadExtractByFileRequestItem shapeFile, int buffer)
    {
        var problems = new List<Problem>();

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
                problems.Add(new ShapeFileInvalidHeader(exception));
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
                                    try
                                    {
                                        polygons.AddRange(GeometryTranslator.ToMultiPolygon(polygonShapeContent.Shape).Geometries.Cast<Polygon>());
                                    }
                                    catch (InvalidPolygonShellOrientationException)
                                    {
                                        problems.Add(new ShapeFileInvalidPolygonShellOrientation());
                                    }
                                    break;

                                default:
                                    problems.Add(new ShapeFileGeometryTypeMustBePolygon());
                                    break;
                            }
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
                    problems.Select(problem =>
                    {
                        var problemTranslation = problem.TranslateToDutch();
                        return new ValidationFailure
                        {
                            PropertyName = nameof(DownloadExtractByFileRequest.ShpFile),
                            ErrorMessage = problemTranslation.Message,
                            ErrorCode = problemTranslation.Code,
                            CustomState = problem.Parameters.ToArray()
                        };
                    }));
            }

            var srid = polygons.First().SRID;
            return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(new MultiPolygon(polygons.ToArray()) { SRID = srid }, buffer);
        }
    }
}


public class DownloadExtractByFileRequestItemTranslatorNetTopologySuite : IDownloadExtractByFileRequestItemTranslator
{
    public RoadNetworkExtractGeometry Translate(DownloadExtractByFileRequestItem shapeFile, int buffer)
    {
        var problems = new List<Problem>();

        ShapefileHeader header = null;
        Geometry geometry = null;
        try
        {
            (header, geometry) = ReadHeaderAndFirstGeometry(shapeFile.ReadStream);
        }
        catch (Exception ex)
        {
            problems.Add(new ShapeFileInvalidHeader(ex));
        }

        var polygons = new List<Polygon>();

        if (header is not null)
        {
            if (new[] { ShapeGeometryType.Polygon, ShapeGeometryType.PolygonM, ShapeGeometryType.PolygonZ, ShapeGeometryType.PolygonZM }.Contains(header.ShapeType))
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

    private (ShapefileHeader, Geometry) ReadHeaderAndFirstGeometry(Stream stream)
    {
        stream.Position = 0;

        var streamProvider = new ExternallyManagedStreamProvider(StreamTypes.Shape, stream);
        var streamProviderRegistry = new ShapefileStreamProviderRegistry(streamProvider, null);

        var shpReader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeReader(streamProviderRegistry);
        var geometry = shpReader.ReadAllShapes(GeometryConfiguration.GeometryFactory).FirstOrDefault();

        return (shpReader.ShapefileHeader, geometry);
    }
}
