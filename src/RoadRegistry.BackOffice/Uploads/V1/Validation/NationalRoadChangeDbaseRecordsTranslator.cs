namespace RoadRegistry.BackOffice.Uploads.V1.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;

public class NationalRoadChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NationalRoadChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<NationalRoadChangeDbaseRecord> records, TranslatedChanges changes)
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
                            new AddRoadSegmentToNationalRoad(
                                records.CurrentRecordNumber,
                                new AttributeId(record.NW_OIDN.Value),
                                new RoadSegmentId(record.WS_OIDN.Value),
                                NationalRoadNumber.Parse(record.IDENT2.Value)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
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
