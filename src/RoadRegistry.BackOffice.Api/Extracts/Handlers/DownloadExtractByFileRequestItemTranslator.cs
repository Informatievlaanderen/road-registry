namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;
using Abstractions.Extracts;
using Core;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Esri;
using ShapeFile.V1;
using ShapeFile.V2;
using GeometryTranslator = GeometryTranslator;
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
                        PropertyName = nameof(DownloadExtractByFileRequest.ShpFile),
                        ErrorMessage = problemTranslation.Message,
                        ErrorCode = problemTranslation.Code
                    }));
        }

        var srid = polygons.First().SRID;
        return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(new MultiPolygon(polygons.ToArray()) { SRID = srid }, buffer);
    }

    private (List<Polygon>, List<Problem>) TryTranslateV1(DownloadExtractByFileRequestItem shapeFile)
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

    private (List<Polygon>, List<Problem>) TryTranslateV2(DownloadExtractByFileRequestItem shapeFile)
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
