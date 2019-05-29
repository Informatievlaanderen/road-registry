namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeChangeShapeRecordsValidator : IZipArchiveShapeRecordsValidator
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var problems = ZipArchiveProblems.None;
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
                            problems = problems.ShapeRecordShapeTypeMismatch(
                                entry.Name,
                                record.Header.RecordNumber,
                                ShapeType.Point,
                                record.Content.ShapeType);
                        }
                        else if (record.Content is PointShapeContent content)
                        {
                            if (!content.Shape.IsValid)
                            {
                                problems = problems.ShapeRecordGeometryMismatch(
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
                    problems = problems.NoShapeRecords(entry.Name);
                }
            }
            catch (Exception exception)
            {
                problems = problems.ShapeRecordFormatError(entry.Name, recordNumber, exception);
            }

            return problems;
        }
    }
}
