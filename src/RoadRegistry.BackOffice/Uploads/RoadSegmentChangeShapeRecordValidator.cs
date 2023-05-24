namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO.Compression;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class RoadSegmentChangeShapeRecordValidator : IZipArchiveShapeRecordValidator
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, RecordNumber recordNumber, Geometry geometry, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var recordContext = entry.AtShapeRecord(recordNumber);
            var lineString = geometry as LineString;
            var multiLineString = geometry as MultiLineString ??
                                  (lineString is not null ? new MultiLineString(new []{ lineString }) : null);
            if (multiLineString is null)
            {
                problems += recordContext.ShapeRecordShapeGeometryTypeMismatch(
                    ShapeGeometryType.LineStringM,
                    geometry.GeometryType);
            }
            else
            {
                var lines = multiLineString
                    .WithMeasureOrdinates()
                    .Geometries
                    .OfType<LineString>()
                    .ToArray();
                if (lines.Length != 1)
                {
                    problems += recordContext.ShapeRecordGeometryLineCountMismatch(
                        1,
                        lines.Length);
                }
                else
                {
                    var line = lines[0];
                    if (line.SelfOverlaps())
                    {
                        problems += recordContext.ShapeRecordGeometrySelfOverlaps();
                    }
                    else if (line.SelfIntersects())
                    {
                        problems += recordContext.ShapeRecordGeometrySelfIntersects();
                    }
                    else if (line.HasInvalidMeasureOrdinates())
                    {
                        problems += recordContext.ShapeRecordGeometryHasInvalidMeasureOrdinates();
                    }
                }
            }
        }
        catch (Exception exception)
        {
            problems += entry.AtShapeRecord(recordNumber).HasShapeRecordFormatError(exception);
        }

        return (problems, context);
    }
}
