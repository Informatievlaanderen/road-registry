namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeChangeShapeRecordsTranslator : IZipArchiveShapeRecordsTranslator
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, TranslatedChanges changes)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(changes);

        while (records.MoveNext())
        {
            var record = records.Current;
            if (record?.Content is PointShapeContent content
                && changes.TryFindRoadNodeChangeOfShapeRecord(record.Header.RecordNumber, out var change))
                switch (change)
                {
                    case AddRoadNode addition:
                        changes = changes.ReplaceChange(addition, addition.WithGeometry(GeometryTranslator.ToPoint(content.Shape)));
                        break;
                    case ModifyRoadNode modification:
                        changes = changes.ReplaceChange(modification, modification.WithGeometry(GeometryTranslator.ToPoint(content.Shape)));
                        break;
                }
        }

        return changes;
    }
}
