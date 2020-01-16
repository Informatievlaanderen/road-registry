namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadNodeChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadNodeChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> records, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            var recordNumber = RecordNumber.Initial;
            while (records.MoveNext())
            {
                var record = records.Current;
                if (record != null)
                {
                    switch (record.RECORDTYPE.Value)
                    {
                        case RecordType.AddedIdentifier:
                            changes = changes.Append(
                                new AddRoadNode(
                                    recordNumber,
                                    new RoadNodeId(record.WEGKNOOPID.Value.GetValueOrDefault()),
                                    RoadNodeType.ByIdentifier[record.TYPE.Value.GetValueOrDefault()]
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
}
