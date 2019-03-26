namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentLaneChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentLaneChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentLaneChangeDbaseRecord> records, TranslatedChanges changes)
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
                            if (changes.TryFindAddRoadSegment(segmentId, out var change))
                            {
                                var lane = new RoadSegmentLaneAttribute(
                                    new AttributeId(record.RS_OIDN.Value.GetValueOrDefault()),
                                    new RoadSegmentLaneCount(record.AANTAL.Value.GetValueOrDefault()),
                                    RoadSegmentLaneDirection.ByIdentifier[record.RICHTING.Value.GetValueOrDefault()],
                                    RoadSegmentPosition.FromDouble(record.VANPOSITIE.Value.GetValueOrDefault()),
                                    RoadSegmentPosition.FromDouble(record.TOTPOSITIE.Value.GetValueOrDefault())
                                );
                                changes = changes.Replace(change, change.WithLane(lane));
                            }
                            break;
                    }
                }
            }

            return changes;
        }
    }
}
