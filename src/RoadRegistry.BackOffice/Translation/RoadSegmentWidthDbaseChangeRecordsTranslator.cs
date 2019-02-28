namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentWidthDbaseChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentWidthChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentWidthChangeDbaseRecord> records, TranslatedChanges changes)
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
                        case RecordTypes.Equal:
                        case RecordTypes.Modified:
                        case RecordTypes.Added:
                            var segmentId = new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault());
                            if (changes.TryFindAddRoadSegment(segmentId, out var before))
                            {
                                var width = new RoadSegmentWidthAttribute(
                                    new AttributeId(record.WB_OIDN.Value.GetValueOrDefault()),
                                    new RoadSegmentWidth(record.BREEDTE.Value.GetValueOrDefault()),
                                    new RoadSegmentPosition(
                                        Convert.ToDecimal(record.VANPOSITIE.Value.GetValueOrDefault())),
                                    new RoadSegmentPosition(
                                        Convert.ToDecimal(record.TOTPOSITIE.Value.GetValueOrDefault()))
                                );
                                changes = changes.Replace(before, before.WithWidth(width));
                            }
                            break;
                    }
                }
            }

            return changes;
        }
    }
}