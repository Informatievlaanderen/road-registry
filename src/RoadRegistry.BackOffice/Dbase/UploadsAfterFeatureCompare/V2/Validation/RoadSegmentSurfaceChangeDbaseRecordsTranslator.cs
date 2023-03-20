namespace RoadRegistry.BackOffice.Dbase.UploadsAfterFeatureCompare.V2.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Uploads;

public class RoadSegmentSurfaceChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentSurfaceChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord> records, TranslatedChanges changes)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(changes);

        while (records.MoveNext())
        {
            var record = records.Current;
            if (record != null)
            {
                var segmentId = new RoadSegmentId(record.WS_OIDN.Value);
                var surface = new RoadSegmentSurfaceAttribute(
                    new AttributeId(record.WV_OIDN.Value),
                    RoadSegmentSurfaceType.ByIdentifier[record.TYPE.Value],
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
                                        modifyRoadSegment.WithSurface(surface));
                                    break;
                                case RecordType.AddedIdentifier:
                                case RecordType.ModifiedIdentifier:
                                    changes = changes.ReplaceChange(modifyRoadSegment,
                                        modifyRoadSegment.WithSurface(surface));
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
                                    changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithSurface(surface));
                                    break;
                                case ModifyRoadSegment modifyRoadSegment:
                                    changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithSurface(surface));
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
