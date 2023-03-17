namespace RoadRegistry.BackOffice.Uploads.Schema.V1;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class EuropeanRoadChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<EuropeanRoadChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord> records, TranslatedChanges changes)
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
                switch (record.RECORDTYPE.Value)
                {
                    case RecordType.AddedIdentifier:
                        changes = changes.AppendChange(
                            new AddRoadSegmentToEuropeanRoad(
                                records.CurrentRecordNumber,
                                new AttributeId(record.EU_OIDN.Value),
                                new RoadSegmentId(record.WS_OIDN.Value),
                                EuropeanRoadNumber.Parse(record.EUNUMMER.Value)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
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
