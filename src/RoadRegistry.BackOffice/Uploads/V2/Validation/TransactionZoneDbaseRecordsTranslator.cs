namespace RoadRegistry.BackOffice.Uploads.Schema.V2;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class TransactionZoneDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<TransactionZoneDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TransactionZoneDbaseRecord> records, TranslatedChanges changes)
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

        if (records.MoveNext() && records.Current != null)
        {
            return changes
                .WithReason(new Reason(records.Current.BESCHRIJV.Value))
                .WithOperatorName(string.IsNullOrEmpty(records.Current.OPERATOR.Value)
                    ? OperatorName.Unknown
                    : new OperatorName(records.Current.OPERATOR.Value))
                .WithOrganization(new OrganizationId(records.Current.ORG.Value));
        }

        return changes;
    }
}
