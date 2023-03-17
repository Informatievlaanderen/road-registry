namespace RoadRegistry.BackOffice.Uploads.Schema.V2;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<GradeSeparatedJunctionChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> records, TranslatedChanges changes)
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
                            new AddGradeSeparatedJunction(
                                records.CurrentRecordNumber,
                                new GradeSeparatedJunctionId(record.OK_OIDN.Value),
                                GradeSeparatedJunctionType.ByIdentifier[record.TYPE.Value],
                                new RoadSegmentId(record.BO_WS_OIDN.Value),
                                new RoadSegmentId(record.ON_WS_OIDN.Value)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
                            new RemoveGradeSeparatedJunction(
                                records.CurrentRecordNumber,
                                new GradeSeparatedJunctionId(record.OK_OIDN.Value)
                            )
                        );
                        break;
                }
            }
        }

        return changes;
    }
}
