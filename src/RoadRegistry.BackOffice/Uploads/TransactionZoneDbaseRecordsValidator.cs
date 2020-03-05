namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class TransactionZoneDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<TransactionZoneDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TransactionZoneDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

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
                            if (!record.BESCHRIJV.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.BESCHRIJV.Field);
                            }
                            if (!record.OPERATOR.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.OPERATOR.Field);
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
                    problems += entry.HasNoDbaseRecords(true);
                }
            }
            catch (Exception exception)
            {
                problems += entry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
            }

            return problems;
        }
    }
}
