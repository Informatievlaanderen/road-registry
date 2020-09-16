namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class EuropeanRoadChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<EuropeanRoadChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord> records, TranslatedChanges changes)
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
                                new AddRoadSegmentToEuropeanRoad(
                                    records.CurrentRecordNumber,
                                    new AttributeId(record.EU_OIDN.Value),
                                    new RoadSegmentId(record.WS_OIDN.Value),
                                    EuropeanRoadNumber.Parse(record.EUNUMMER.Value)
                                )
                            );
                            break;
                        case RecordType.ModifiedIdentifier:
                            // TODO: What would this even mean? There are no meaningful attributes to change.
                            break;
                        case RecordType.RemovedIdentifier:
                            //TODO: This will only work if WS_OIDN remains stable (is this a fair assumption)
                            changes = changes.Append(
                                new RemoveRoadSegmentFromEuropeanRoad(
                                    records.CurrentRecordNumber,
                                    new AttributeId(record.EU_OIDN.Value),
                                    new RoadSegmentId(record.WS_OIDN.Value),
                                    EuropeanRoadNumber.Parse(record.EUNUMMER.Value)
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
