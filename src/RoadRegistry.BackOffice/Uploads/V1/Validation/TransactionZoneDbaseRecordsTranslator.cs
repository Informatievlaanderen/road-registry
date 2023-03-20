namespace RoadRegistry.BackOffice.Uploads.V1.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Polly;
using Schema;

public class TransactionZoneDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<TransactionZoneDbaseRecord>
{
    public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TransactionZoneDbaseRecord> records, TranslatedChanges changes)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(changes);

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
