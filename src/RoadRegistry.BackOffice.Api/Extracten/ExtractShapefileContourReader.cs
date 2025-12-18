namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandHandling;
using FluentValidation;
using FluentValidation.Results;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Esri;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.ShapeFile.V1;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Infrastructure;
using ValueObjects.Problems;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Problem = ValueObjects.Problems.Problem;

public interface IExtractShapefileContourReader
{
    Geometry Read(Stream shpStream);
}

public class ExtractShapefileContourReader : IExtractShapefileContourReader
{
    public Geometry Read(Stream shpStream)
    {
        var (polygons, problems) = TryTranslateV2(shpStream);
        if (problems.Any())
        {
            var (v1Polygons, v1Problems) = TryTranslateV1(shpStream);
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

    private (List<Polygon>, List<Problem>) TryTranslateV1(Stream shpStream)
    {
        var problems = new List<Problem>();

        ShapefileHeader header = null;
        Geometry geometry = null;
        try
        {
            (header, geometry) = new ExtractGeometryShapeFileReaderV1().Read(shpStream);
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

    private (List<Polygon>, List<Problem>) TryTranslateV2(Stream shpStream)
    {
        var problems = new List<Problem>();

        ShapeType? shapeType = null;
        Geometry geometry = null;
        try
        {
            (shapeType, geometry) = new ExtractGeometryShapeFileReaderV2().Read(shpStream);
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
