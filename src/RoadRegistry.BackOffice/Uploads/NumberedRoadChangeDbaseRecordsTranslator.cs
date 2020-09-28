namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class NumberedRoadChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NumberedRoadChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord> records, TranslatedChanges changes)
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
                        case RecordType.AddedIdentifier:
                            changes = changes.Append(
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
                        case RecordType.ModifiedIdentifier:
                            //TODO: This will only work if WS_OIDN remains the same (is this a fair assumption)
                            changes = changes.Append(
                                new ModifyRoadSegmentOnNumberedRoad(
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
                            //TODO: This will only work if WS_OIDN remains stable (is this a fair assumption)
                            changes = changes.Append(
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
}
