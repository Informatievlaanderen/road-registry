namespace RoadRegistry.BackOffice.Uploads.V1.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;

public class RoadSegmentLaneChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentLaneChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentLaneChangeDbaseRecord> records, TranslatedChanges changes)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (records == null)
        {
            throw new ArgumentNullException(nameof(records));
        }

        if (changes == null)
        {
            throw new ArgumentNullException(nameof(changes));
        }

        while (records.MoveNext())
        {
            var record = records.Current;
            if (record != null)
            {
                var segmentId = new RoadSegmentId(record.WS_OIDN.Value);
                var lane = new RoadSegmentLaneAttribute(
                    new AttributeId(record.RS_OIDN.Value),
                    new RoadSegmentLaneCount(record.AANTAL.Value),
                    RoadSegmentLaneDirection.ByIdentifier[record.RICHTING.Value],
                    RoadSegmentPosition.FromDouble(record.VANPOSITIE.Value),
                    RoadSegmentPosition.FromDouble(record.TOTPOSITIE.Value)
                );
                if (changes.TryFindRoadSegmentProvisionalChange(segmentId, out var provisionalChange))
                {
                    switch (provisionalChange)
                    {
                        case ModifyRoadSegment modifyRoadSegment:
                            switch (record.RECORDTYPE.Value)
                            {
                                case RecordType.IdenticalIdentifier:
                                    changes = changes.ReplaceProvisionalChange(modifyRoadSegment,
                                        modifyRoadSegment.WithLane(lane));
                                    break;
                                case RecordType.AddedIdentifier:
                                case RecordType.ModifiedIdentifier:
                                    changes = changes.ReplaceChange(modifyRoadSegment,
                                        modifyRoadSegment.WithLane(lane));
                                    break;
                                case RecordType.RemovedIdentifier:
                                    changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment);
                                    break;
                            }

                            break;
                    }
                }
                else if (changes.TryFindRoadSegmentChange(segmentId, out var change))
                {
                    switch (record.RECORDTYPE.Value)
                    {
                        case RecordType.IdenticalIdentifier:
                        case RecordType.AddedIdentifier:
                            switch (change)
                            {
                                case AddRoadSegment addRoadSegment:
                                    changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithLane(lane));
                                    break;
                                case ModifyRoadSegment modifyRoadSegment:
                                    changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithLane(lane));
                                    break;
                            }

                            break;
                    }
                }
            }
        }

        return changes;
    }
}
