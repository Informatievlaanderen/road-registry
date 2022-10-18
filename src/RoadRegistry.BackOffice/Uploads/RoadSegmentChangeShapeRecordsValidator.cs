namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using NetTopologySuite.Geometries;

public class RoadSegmentChangeShapeRecordsValidator : IZipArchiveShapeRecordsValidator
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, ZipArchiveValidationContext context)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (records == null) throw new ArgumentNullException(nameof(records));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = ZipArchiveProblems.None;
        var recordNumber = RecordNumber.Initial;
        try
        {
            var moved = records.MoveNext();
            if (moved)
                while (moved)
                {
                    var record = records.Current;
                    if (record != null)
                    {
                        var recordContext = entry.AtShapeRecord(record.Header.RecordNumber);
                        if (record.Content.ShapeType != ShapeType.PolyLineM)
                        {
                            problems += recordContext.ShapeRecordShapeTypeMismatch(
                                ShapeType.PolyLineM,
                                record.Content.ShapeType);
                        }
                        else if (record.Content is PolyLineMShapeContent content)
                        {
                            var shape = GeometryTranslator.ToGeometryMultiLineString(content.Shape);
                            if (!shape.IsValid)
                            {
                                problems += recordContext.ShapeRecordGeometryMismatch();
                            }
                            else
                            {
                                var lines = shape
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
                                        problems += recordContext.ShapeRecordGeometrySelfOverlaps();
                                    else if (line.SelfIntersects()) problems += recordContext.ShapeRecordGeometrySelfIntersects();
                                }
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
            else
                problems += entry.HasNoShapeRecords();
        }
        catch (Exception exception)
        {
            problems += entry.AtShapeRecord(recordNumber).HasShapeRecordFormatError(exception);
        }

        return (problems, context);
    }
}