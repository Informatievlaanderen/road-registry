namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class NumberedRoadChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NumberedRoadChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord> records, TranslatedChanges changes)
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
                                new AddRoadSegmentToNumberedRoad(
                                    new AttributeId(record.GW_OIDN.Value.GetValueOrDefault()),
                                    new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
                                    NumberedRoadNumber.Parse(record.IDENT8.Value),
                                    RoadSegmentNumberedRoadDirection.ByIdentifier[record.RICHTING.Value.GetValueOrDefault()],
                                    new RoadSegmentNumberedRoadOrdinal(record.VOLGNUMMER.Value.GetValueOrDefault())
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