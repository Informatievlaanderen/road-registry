namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeShapeRecordValidator : IZipArchiveShapeRecordValidator
    {
        public ZipArchiveErrors Validate(ZipArchiveEntry entry, ShapeRecord record)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var errors = ZipArchiveErrors.None;

            if (record.Content.ShapeType != ShapeType.Point)
            {
                errors = errors.ShapeRecordShapeTypeMismatch(
                    entry.Name,
                    record.Header.RecordNumber,
                    ShapeType.Point,
                    record.Content.ShapeType);
            }

            if (record.Content is PointShapeContent content)
            {
                if (content.Shape.IsEmpty)
                {
                    errors = errors.ShapeRecordGeometryMismatch(
                        entry.Name,
                        record.Header.RecordNumber);
                }
            }

            return errors;
        }
    }
}
