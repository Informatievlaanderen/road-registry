namespace RoadRegistry.BackOffice.Uploads.Schema.V2;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadNodeChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> records, TranslatedChanges changes)
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
