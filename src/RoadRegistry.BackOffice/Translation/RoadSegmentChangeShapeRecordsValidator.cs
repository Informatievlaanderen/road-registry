namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using NetTopologySuite.Geometries;

    public class RoadSegmentChangeShapeRecordsValidator : IZipArchiveShapeRecordsValidator
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
                        if (record.Content.ShapeType != ShapeType.PolyLineM)
                        {
                            problems = problems.ShapeRecordShapeTypeMismatch(
                                entry.Name,
                                record.Header.RecordNumber,
                                ShapeType.PolyLineM,
                                record.Content.ShapeType);
                        }
                        else if (record.Content is PolyLineMShapeContent content)
                        {
                            var shape = GeometryTranslator.ToGeometryMultiLineString(content.Shape);
                            if (!shape.IsValid)
                            {
                                problems = problems.ShapeRecordGeometryMismatch(
                                    entry.Name,
                                    record.Header.RecordNumber);
                            }
                            else
                            {
                                var lines = shape
                                    .Geometries
                                    .OfType<LineString>()
                                    .ToArray();
                                if (lines.Length != 1)
                                {
                                    problems = problems.ShapeRecordGeometryLineCountMismatch(
                                        entry.Name,
                                        record.Header.RecordNumber,
                                        1,
                                        lines.Length);
                                }
                                else
                                {
                                    var line = lines[0];
                                    if (Model.LineStringExtensions.SelfOverlaps(line))
                                    {
                                        problems = problems.ShapeRecordGeometrySelfOverlaps(
                                            entry.Name,
                                            record.Header.RecordNumber);
                                    }
                                    else if (Model.LineStringExtensions.SelfIntersects(line))
                                    {
                                        problems = problems.ShapeRecordGeometrySelfIntersects(
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
