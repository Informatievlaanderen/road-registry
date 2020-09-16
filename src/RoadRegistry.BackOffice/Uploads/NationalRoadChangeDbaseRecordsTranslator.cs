namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class NationalRoadChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NationalRoadChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<NationalRoadChangeDbaseRecord> records, TranslatedChanges changes)
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
                                new AddRoadSegmentToNationalRoad(
                                    records.CurrentRecordNumber,
                                    new AttributeId(record.NW_OIDN.Value),
                                    new RoadSegmentId(record.WS_OIDN.Value),
                                    NationalRoadNumber.Parse(record.IDENT2.Value)
                                )
                            );
                            break;
                        case RecordType.ModifiedIdentifier:
                            // TODO: What would this even mean? There are no meaningful attributes to change.
                            break;
                        case RecordType.RemovedIdentifier:
                            //TODO: This will only work if WS_OIDN remains stable (is this a fair assumption)
                            changes = changes.Append(
                                new RemoveRoadSegmentFromNationalRoad(
                                    records.CurrentRecordNumber,
                                    new AttributeId(record.NW_OIDN.Value),
                                    new RoadSegmentId(record.WS_OIDN.Value),
                                    NationalRoadNumber.Parse(record.IDENT2.Value)
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
