namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.Text;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Editor.Projections.DutchTranslations;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using NetTopologySuite.Geometries;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Problem = Core.Problem;

public class DownloadExtractByFileRequestItemTranslator
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
                        if (srids.Length > 1) problems.Add(new ShapeFileGeometrySridMustBeEqual());
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
                throw new ValidationException("Shape file content contains some errors",
                    problems.Select(problem =>
                        new ValidationFailure(nameof(DownloadExtractByFileRequest.ShpFile),
                            ProblemWithDownload.Translator(problem))));

            var srid = polygons.First().SRID;
            return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(new MultiPolygon(polygons.ToArray()) { SRID = srid }, buffer);
        }
    }
}
