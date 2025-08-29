namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Esri;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.ShapeFile.V1;
using RoadRegistry.BackOffice.ShapeFile.V2;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Problem = Core.Problem;

public interface IDownloadExtractByFileRequestItemTranslator
{
    Geometry Translate(ExtractDownloadaanvraagPerBestandItem shapeFile);
}

public class DownloadExtractByFileRequestItemTranslator : IDownloadExtractByFileRequestItemTranslator
{
    public Geometry Translate(ExtractDownloadaanvraagPerBestandItem shapeFile)
    {
        var (polygons, problems) = TryTranslateV2(shapeFile);
        if (problems.Any())
        {
            var (v1Polygons, v1Problems) = TryTranslateV1(shapeFile);
            if (!v1Problems.Any())
            {
                polygons = v1Polygons;
                problems.Clear();
            }
        }

        if (problems.Any())
        {
            throw new ValidationException("Shape file content contains some errors",
                problems
                    .Select(problem => problem.TranslateToDutch())
                    .Select(problemTranslation => new ValidationFailure
                    {
                        PropertyName = nameof(ExtractDownloadaanvraagPerBestand.ShpFile),
                        ErrorMessage = problemTranslation.Message,
                        ErrorCode = problemTranslation.Code
                    }));
        }

        var srid = polygons.First().SRID;
        return new MultiPolygon(polygons.ToArray()) { SRID = srid };
    }

    private (List<Polygon>, List<Problem>) TryTranslateV1(ExtractDownloadaanvraagPerBestandItem shapeFile)
    {
        var problems = new List<Problem>();

        ShapefileHeader header = null;
        Geometry geometry = null;
        try
        {
            (header, geometry) = new ExtractGeometryShapeFileReaderV1().Read(shapeFile.ReadStream);
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

        return (polygons, problems);
    }

    private (List<Polygon>, List<Problem>) TryTranslateV2(ExtractDownloadaanvraagPerBestandItem shapeFile)
    {
        var problems = new List<Problem>();

        ShapeType? shapeType = null;
        Geometry geometry = null;
        try
        {
            (shapeType, geometry) = new ExtractGeometryShapeFileReaderV2().Read(shapeFile.ReadStream);
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

        return (polygons, problems);
    }
}
