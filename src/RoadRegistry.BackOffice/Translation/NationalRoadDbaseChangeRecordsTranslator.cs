namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Model;

    public class NationalRoadDbaseChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NationalRoadChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<NationalRoadChangeDbaseRecord> records, TranslatedChanges changes)
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
                        case RecordTypes.Added:
                            changes = changes.Append(
                                new AddRoadSegmentToNationalRoad(
                                    new AttributeId(record.NW_OIDN.Value.GetValueOrDefault()),
                                    new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
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