namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using NetTopologySuite.Geometries;

    public class RoadSegmentChangeShapeRecordsValidator : IZipArchiveShapeRecordsValidator
    {
        public ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var errors = ZipArchiveErrors.None;
            var recordNumber = RecordNumber.Initial;
            try
            {
                var count = 0;
                while (records.MoveNext())
                {
                    var record = records.Current;
                    if (record != null)
                    {
                        if (record.Content.ShapeType != ShapeType.PolyLineM)
                        {
                            errors = errors.ShapeRecordShapeTypeMismatch(
                                entry.Name,
                                record.Header.RecordNumber,
                                ShapeType.PolyLineM,
                                record.Content.ShapeType);
                        }
                        else if (record.Content is PolyLineMShapeContent content)
                        {
                            if (!content.Shape.IsValid)
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
                    }

                    count++;
                    recordNumber = recordNumber.Next();
                }

                if (count == 0)
                {
                    errors = errors.NoShapeRecords(entry.Name);
                }
            }
            catch (Exception exception)
            {
                errors = errors.ShapeRecordFormatError(entry.Name, recordNumber, exception);
            }

            return errors;
        }
    }
}
