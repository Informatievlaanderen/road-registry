namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

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
                        case RecordType.IdenticalIdentifier:
                        case RecordType.AddedIdentifier:
                            var segmentId = new RoadSegmentId(record.WS_OIDN.Value);
                            if (changes.TryFindRoadSegmentChange(segmentId, out var change))
                            {
                                var lane = new RoadSegmentLaneAttribute(
                                    new AttributeId(record.RS_OIDN.Value),
                                    new RoadSegmentLaneCount(record.AANTAL.Value),
                                    RoadSegmentLaneDirection.ByIdentifier[record.RICHTING.Value],
                                    RoadSegmentPosition.FromDouble(record.VANPOSITIE.Value),
                                    RoadSegmentPosition.FromDouble(record.TOTPOSITIE.Value)
                                );

                                switch (change)
                                {
                                    case AddRoadSegment addRoadSegment:
                                        changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithLane(lane));
                                        break;
                                    case ModifyRoadSegment modifyRoadSegment:
                                        changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithLane(lane));
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            return changes;
        }
    }
}
