namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentChangeDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> records, TranslatedChanges changes)
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
                                new AddRoadSegment(
                                    records.CurrentRecordNumber,
                                    new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
                                    new RoadNodeId(record.B_WK_OIDN.Value.GetValueOrDefault()),
                                    new RoadNodeId(record.E_WK_OIDN.Value.GetValueOrDefault()),
                                    new MaintenanceAuthorityId(record.BEHEERDER.Value),
                                    RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value.GetValueOrDefault()],
                                    RoadSegmentMorphology.ByIdentifier[record.MORFOLOGIE.Value.GetValueOrDefault()],
                                    RoadSegmentStatus.ByIdentifier[record.STATUS.Value.GetValueOrDefault()],
                                    RoadSegmentCategory.ByIdentifier[record.WEGCAT.Value],
                                    RoadSegmentAccessRestriction.ByIdentifier[record.TGBEP.Value.GetValueOrDefault()],
                                    record.LSTRNMID.Value.HasValue ? new CrabStreetnameId(record.LSTRNMID.Value.Value) : new CrabStreetnameId?(),
                                    record.RSTRNMID.Value.HasValue ? new CrabStreetnameId(record.RSTRNMID.Value.Value) : new CrabStreetnameId?()
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
