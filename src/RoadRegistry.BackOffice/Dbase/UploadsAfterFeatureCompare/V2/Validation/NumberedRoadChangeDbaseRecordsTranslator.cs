namespace RoadRegistry.BackOffice.Dbase.UploadsAfterFeatureCompare.V2.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Uploads;

public class NumberedRoadChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NumberedRoadChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord> records, TranslatedChanges changes)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(changes);

        while (records.MoveNext())
        {
            var record = records.Current;
            if (record != null)
            {
                switch (record.RECORDTYPE.Value)
                {
                    case RecordType.AddedIdentifier:
                        changes = changes.AppendChange(
                            new AddRoadSegmentToNumberedRoad(
                                records.CurrentRecordNumber,
                                new AttributeId(record.GW_OIDN.Value),
                                new RoadSegmentId(record.WS_OIDN.Value),
                                NumberedRoadNumber.Parse(record.IDENT8.Value),
                                RoadSegmentNumberedRoadDirection.ByIdentifier[record.RICHTING.Value],
                                new RoadSegmentNumberedRoadOrdinal(record.VOLGNUMMER.Value)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
                            new RemoveRoadSegmentFromNumberedRoad(
                                records.CurrentRecordNumber,
                                new AttributeId(record.GW_OIDN.Value),
                                new RoadSegmentId(record.WS_OIDN.Value),
                                NumberedRoadNumber.Parse(record.IDENT8.Value)
                            )
                        );
                        break;
                }
            }
        }

        return changes;
    }
}
