namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class RoadNodeChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadNodeChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> records, TranslatedChanges changes)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(changes);

        var recordNumber = RecordNumber.Initial;
        while (records.MoveNext())
        {
            var record = records.Current;
            if (record != null)
            {
                switch (record.RECORDTYPE.Value)
                {
                    case RecordType.AddedIdentifier:
                        changes = changes.AppendChange(
                            new AddRoadNode(
                                recordNumber,
                                new RoadNodeId(record.WEGKNOOPID.Value),
                                RoadNodeType.ByIdentifier[record.TYPE.Value]
                            )
                        );
                        break;
                    case RecordType.ModifiedIdentifier:
                        changes = changes.AppendChange(
                            new ModifyRoadNode(
                                recordNumber,
                                new RoadNodeId(record.WEGKNOOPID.Value),
                                RoadNodeType.ByIdentifier[record.TYPE.Value]
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
                            new RemoveRoadNode(
                                recordNumber,
                                new RoadNodeId(record.WEGKNOOPID.Value)
                            )
                        );
                        break;
                }
            }

            recordNumber = recordNumber.Next();
        }

        return changes;
    }
}
