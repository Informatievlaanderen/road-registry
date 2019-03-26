namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentSurfaceChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentSurfaceChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord> records, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            while (records.MoveNext())
            {
                var record = records.Current;
                if (record != null)
                {
                    switch (record.RECORDTYPE.Value)
                    {
                        case RecordTypes.Equal:
                        case RecordTypes.Modified:
                        case RecordTypes.Added:
                            var segmentId = new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault());
                            if (changes.TryFindAddRoadSegment(segmentId, out var before))
                            {
                                var surface = new RoadSegmentSurfaceAttribute(
                                    new AttributeId(record.WV_OIDN.Value.GetValueOrDefault()),
                                    RoadSegmentSurfaceType.ByIdentifier[record.TYPE.Value.GetValueOrDefault()],
                                    RoadSegmentPosition.FromDouble(record.VANPOSITIE.Value.GetValueOrDefault()),
                                    RoadSegmentPosition.FromDouble(record.TOTPOSITIE.Value.GetValueOrDefault())
                                );
                                changes = changes.Replace(before, before.WithSurface(surface));
                            }
                            break;
                    }
                }
            }

            return changes;
        }
    }
}
