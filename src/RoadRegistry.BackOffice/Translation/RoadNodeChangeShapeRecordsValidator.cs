namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeChangeShapeRecordsValidator : IZipArchiveShapeRecordsValidator
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
                        if (record.Content.ShapeType != ShapeType.Point)
                        {
                            errors = errors.ShapeRecordShapeTypeMismatch(
                                entry.Name,
                                record.Header.RecordNumber,
                                ShapeType.Point,
                                record.Content.ShapeType);
                        }
                        else if (record.Content is PointShapeContent content)
                        {
                            if (!content.Shape.IsValid)
                            {
                                errors = errors.ShapeRecordGeometryMismatch(
                                    entry.Name,
                                    record.Header.RecordNumber);
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
