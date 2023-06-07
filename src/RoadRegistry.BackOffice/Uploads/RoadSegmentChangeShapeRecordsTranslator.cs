namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentChangeShapeRecordsTranslator : IZipArchiveShapeRecordsTranslator
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, TranslatedChanges changes)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(changes);

        while (records.MoveNext())
        {
            var record = records.Current;
            if (record != null && record.Content is PolyLineMShapeContent content)
            {
                var geometry = GeometryTranslator.ToMultiLineString(content.Shape);
                if (changes.TryFindRoadSegmentProvisionalChange(record.Header.RecordNumber,
                        out var provisionalChange))
                    switch (provisionalChange)
                    {
                        case ModifyRoadSegment modifyRoadSegment:
                            changes = changes.ReplaceProvisionalChange(modifyRoadSegment, modifyRoadSegment.WithGeometry(geometry));
                            break;
                    }
                else if (changes.TryFindRoadSegmentChange(record.Header.RecordNumber, out var change))
                    switch (change)
                    {
                        case AddRoadSegment addRoadSegment:
                            changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithGeometry(geometry));
                            break;
                        case ModifyRoadSegment modifyRoadSegment:
                            changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithGeometry(geometry));
                            break;
                    }
            }
        }

        return changes;
    }
}
