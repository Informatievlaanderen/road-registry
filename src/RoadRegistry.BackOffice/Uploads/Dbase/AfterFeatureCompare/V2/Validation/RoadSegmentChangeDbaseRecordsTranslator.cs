namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class
    RoadSegmentChangeDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentChangeDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry,
        IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> records, TranslatedChanges changes)
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
                            new AddRoadSegment(
                                records.CurrentRecordNumber,
                                record.EVENTIDN.HasValue && record.EVENTIDN.Value != 0
                                    ? new RoadSegmentId(record.EVENTIDN.Value)
                                    : new RoadSegmentId(record.WS_OIDN.Value),
                                new RoadSegmentId(record.WS_OIDN.Value),
                                new RoadNodeId(record.B_WK_OIDN.Value),
                                new RoadNodeId(record.E_WK_OIDN.Value),
                                new OrganizationId(record.BEHEERDER.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value],
                                RoadSegmentMorphology.ByIdentifier[record.MORFOLOGIE.Value],
                                RoadSegmentStatus.ByIdentifier[record.STATUS.Value],
                                RoadSegmentCategory.ByIdentifier[record.CATEGORIE.Value],
                                RoadSegmentAccessRestriction.ByIdentifier[record.TGBEP.Value],
                                record.LSTRNMID.Value.HasValue
                                    ? new CrabStreetNameId(record.LSTRNMID.Value.Value)
                                    : new CrabStreetNameId?(),
                                record.RSTRNMID.Value.HasValue
                                    ? new CrabStreetNameId(record.RSTRNMID.Value.Value)
                                    : new CrabStreetNameId?()
                            )
                        );
                        break;
                    case RecordType.IdenticalIdentifier:
                        changes = changes.AppendProvisionalChange(
                            new ModifyRoadSegment(
                                records.CurrentRecordNumber,
                                new RoadSegmentId(record.WS_OIDN.Value),
                                new RoadNodeId(record.B_WK_OIDN.Value),
                                new RoadNodeId(record.E_WK_OIDN.Value),
                                new OrganizationId(record.BEHEERDER.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value],
                                RoadSegmentMorphology.ByIdentifier[record.MORFOLOGIE.Value],
                                RoadSegmentStatus.ByIdentifier[record.STATUS.Value],
                                RoadSegmentCategory.ByIdentifier[record.CATEGORIE.Value],
                                RoadSegmentAccessRestriction.ByIdentifier[record.TGBEP.Value],
                                record.LSTRNMID.Value.HasValue ? new CrabStreetNameId(record.LSTRNMID.Value.Value) : new CrabStreetNameId?(),
                                record.RSTRNMID.Value.HasValue ? new CrabStreetNameId(record.RSTRNMID.Value.Value) : new CrabStreetNameId?()
                            )
                        );
                        break;
                    case RecordType.ModifiedIdentifier:
                        changes = changes.AppendChange(
                            new ModifyRoadSegment(
                                records.CurrentRecordNumber,
                                new RoadSegmentId(record.WS_OIDN.Value),
                                new RoadNodeId(record.B_WK_OIDN.Value),
                                new RoadNodeId(record.E_WK_OIDN.Value),
                                new OrganizationId(record.BEHEERDER.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value],
                                RoadSegmentMorphology.ByIdentifier[record.MORFOLOGIE.Value],
                                RoadSegmentStatus.ByIdentifier[record.STATUS.Value],
                                RoadSegmentCategory.ByIdentifier[record.CATEGORIE.Value],
                                RoadSegmentAccessRestriction.ByIdentifier[record.TGBEP.Value],
                                record.LSTRNMID.Value.HasValue
                                    ? new CrabStreetNameId(record.LSTRNMID.Value.Value)
                                    : new CrabStreetNameId?(),
                                record.RSTRNMID.Value.HasValue
                                    ? new CrabStreetNameId(record.RSTRNMID.Value.Value)
                                    : new CrabStreetNameId?()
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
                            new RemoveRoadSegment(
                                records.CurrentRecordNumber,
                                new RoadSegmentId(record.WS_OIDN.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value]
                            )
                        );
                        break;
                }
            }
        }

        return changes;
    }
}
