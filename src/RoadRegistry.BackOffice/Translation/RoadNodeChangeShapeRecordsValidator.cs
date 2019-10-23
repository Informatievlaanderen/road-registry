namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

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
                var moved = records.MoveNext();
                if (moved)
                {
                    while (moved)
                    {
                        var record = records.Current;
                        if (record != null)
                        {
                            var recordContext = entry.AtShapeRecord(record.Header.RecordNumber);
                            if (record.Content.ShapeType != ShapeType.Point)
                            {
                                problems += recordContext.ShapeRecordShapeTypeMismatch(
                                    ShapeType.Point,
                                    record.Content.ShapeType);
                            }
                            else if (record.Content is PointShapeContent content)
                            {
                                if (!GeometryTranslator.ToGeometryPoint(content.Shape).IsValid)
                                {
                                    problems += recordContext.ShapeRecordGeometryMismatch();
                                }
                            }
                            recordNumber = record.Header.RecordNumber.Next();
                        }
                        else
                        {
                            recordNumber = recordNumber.Next();
                        }

                        moved = records.MoveNext();
                    }
                }
                else
                {
                    problems += entry.HasNoShapeRecords();
                }
            }
            catch (Exception exception)
            {
                problems += entry.AtShapeRecord(recordNumber).HasShapeRecordFormatError(exception);
            }

            return problems;
        }
    }
}
