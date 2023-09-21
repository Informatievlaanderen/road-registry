namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class TransactionZoneDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<TransactionZoneDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TransactionZoneDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var count = 0;
            var moved = records.MoveNext();
            if (moved)
            {
                while (moved)
                {
                    var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                    var record = records.Current;
                    if (record != null)
                    {
                        if (string.IsNullOrEmpty(record.BESCHRIJV.Value))
                        {
                            problems += recordContext.RequiredFieldIsNull(record.BESCHRIJV.Field);
                        }

                        if (!record.OPERATOR.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.OPERATOR.Field);
                        }

                        if (!record.DOWNLOADID.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.DOWNLOADID.Field);
                        }
                        else if (!DownloadId.CanParse(record.DOWNLOADID.Value))
                        {
                            problems += recordContext.DownloadIdInvalidFormat(record.DOWNLOADID.Value);
                        }
                        else
                        {
                            var expectedDownloadId = context.ZipArchiveMetadata.DownloadId;
                            if (expectedDownloadId.HasValue
                                && !DownloadId.Parse(record.DOWNLOADID.Value).Equals(expectedDownloadId.Value))
                            {
                                problems += recordContext.DownloadIdDiffersFromMetadata(record.DOWNLOADID.Value, expectedDownloadId.ToString());
                            }
                        }

                        if (!record.ORG.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.ORG.Field);
                        }
                        else if (!OrganizationId.AcceptsValue(record.ORG.Value))
                        {
                            problems += recordContext.OrganizationIdOutOfRange(record.ORG.Value);
                        }

                        count++;
                        moved = records.MoveNext();
                    }
                }

                if (count != 1)
                {
                    problems += entry.HasTooManyDbaseRecords(1, count);
                }
            }
            else
            {
                problems += entry.HasNoDbaseRecords();
            }
        }
        catch (Exception exception)
        {
            problems += entry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
        }

        return (problems, context);
    }
}
