namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using NetTopologySuite.Geometries;

    public class RoadSegmentShapeRecordValidator : IZipArchiveShapeRecordValidator
    {
        public ZipArchiveErrors Validate(ZipArchiveEntry entry, ShapeRecord record)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var errors = ZipArchiveErrors.None;

            if (record.Content.ShapeType != ShapeType.PolyLineM)
            {
                errors = errors.ShapeRecordShapeTypeMismatch(
                    entry.Name,
                    record.Header.RecordNumber,
                    ShapeType.PolyLineM,
                    record.Content.ShapeType);
            }

            if (record.Content is PolyLineMShapeContent content)
            {
                if (content.Shape.IsEmpty)
                {
                    errors = errors.ShapeRecordGeometryMismatch(
                        entry.Name,
                        record.Header.RecordNumber);
                }
                else
                {
                    var lines = content.Shape
                        .Geometries
                        .OfType<LineString>()
                        .ToArray();
                    if (lines.Length != 1)
                    {
                        errors = errors.ShapeRecordGeometryLineCountMismatch(
                            entry.Name,
                            record.Header.RecordNumber,
                            1,
                            lines.Length);
                    }
                    else
                    {
                        var line = lines[0];
                        if (line.SelfOverlaps())
                        {
                            errors = errors.ShapeRecordGeometrySelfOverlaps(
                                entry.Name,
                                record.Header.RecordNumber);
                        }
                        else if (line.SelfIntersects())
                        {
                            errors = errors.ShapeRecordGeometrySelfIntersects(
                                entry.Name,
                                record.Header.RecordNumber);
                        }
                    }
                }
            }

            return errors;
        }
    }
}
