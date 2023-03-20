namespace RoadRegistry.BackOffice.Dbase.UploadsAfterFeatureCompare.V1.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Uploads;

public class GradeSeparatedJunctionChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<GradeSeparatedJunctionChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> records, TranslatedChanges changes)
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
