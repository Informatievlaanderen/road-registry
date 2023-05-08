namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.IO.Compression;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeChangeShapeRecordValidator : IZipArchiveShapeRecordValidator
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, RecordNumber recordNumber, Geometry geometry, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;

        var recordContext = entry.AtShapeRecord(recordNumber);
        if (geometry is not Point)
        {
            problems += recordContext.ShapeRecordShapeGeometryTypeMismatch(
                ShapeGeometryType.Point,
                geometry.GeometryType);
        }

        return (problems, context);
    }
}
