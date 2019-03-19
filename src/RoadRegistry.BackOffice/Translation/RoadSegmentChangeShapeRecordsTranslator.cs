namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentChangeShapeRecordsTranslator : IZipArchiveShapeRecordsTranslator
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (changes == null) throw new ArgumentNullException(nameof(changes));
            while (records.MoveNext())
            {
                var record = records.Current;
                if (record != null && record.Content is PolyLineMShapeContent content)
                {
                    if (changes.TryTranslateToRoadSegmentId(record.Header.RecordNumber, out var id))
                    {
                        if (changes.TryFindAddRoadSegment(id, out var change))
                        {
                            changes = changes.Replace(change, change.WithGeometry(content.Shape));
                        }
                    }
                }
            }
            return changes;
        }
    }
}