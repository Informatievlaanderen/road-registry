namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class GradeSeparatedJunctionChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<GradeSeparatedJunctionChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> records, TranslatedChanges changes)
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
                                new AddGradeSeparatedJunction(
                                    new GradeSeparatedJunctionId(record.OK_OIDN.Value.GetValueOrDefault()),
                                    GradeSeparatedJunctionType.ByIdentifier[record.TYPE.Value.GetValueOrDefault()],
                                    new RoadSegmentId(record.BO_WS_OIDN.Value.GetValueOrDefault()),
                                    new RoadSegmentId(record.ON_WS_OIDN.Value.GetValueOrDefault())
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
