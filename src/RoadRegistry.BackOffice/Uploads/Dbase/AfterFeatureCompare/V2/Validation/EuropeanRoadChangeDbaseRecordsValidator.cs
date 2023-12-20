namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class EuropeanRoadChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<EuropeanRoadChangeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var identifiers = new Dictionary<AttributeId, RecordNumber>();
            var moved = records.MoveNext();
            if (moved)
            {
                while (moved)
                {
                    var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                    var record = records.Current;
                    if (record != null)
                    {
                        RecordType recordType = default;
                        if (!record.RECORDTYPE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.RECORDTYPE.Field);
                        }
                        else
                        {
                            if (!RecordType.ByIdentifier.TryGetValue(record.RECORDTYPE.Value, out recordType))
                            {
                                problems += recordContext.RecordTypeMismatch(record.RECORDTYPE.Value);
                            }
                            else if (!recordType.IsAnyOf(RecordType.Identical, RecordType.Added, RecordType.Removed))
                            {
                                problems += recordContext.RecordTypeNotSupported(record.RECORDTYPE.Value, RecordType.Identical.Translation.Identifier, RecordType.Added.Translation.Identifier, RecordType.Removed.Translation.Identifier);
                            }
                        }

                        if (record.EU_OIDN.HasValue)
                        {
                            if (record.EU_OIDN.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                            else
                            {
                                if (recordType == RecordType.Added)
                                {
                                    var identifier = new AttributeId(record.EU_OIDN.Value);
                                    if (identifiers.TryGetValue(identifier, out var takenByRecordNumber))
                                    {
                                        problems += recordContext.IdentifierNotUnique(identifier,
                                            takenByRecordNumber);
                                    }
                                    else
                                    {
                                        identifiers.Add(identifier, records.CurrentRecordNumber);
                                    }
                                }
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.EU_OIDN.Field);
                        }

                        if (!record.EUNUMMER.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.EUNUMMER.Field);
                        }
                        else if (!EuropeanRoadNumber.CanParse(record.EUNUMMER.Value))
                        {
                            problems += recordContext.NotEuropeanRoadNumber(record.EUNUMMER.Value);
                        }

                        if (!record.WS_OIDN.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WS_OIDN.Field);
                        }
                        else if (!RoadSegmentId.Accepts(record.WS_OIDN.Value))
                        {
                            problems += recordContext.RoadSegmentIdOutOfRange(record.WS_OIDN.Value);
                        }
                    }

                    moved = records.MoveNext();
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
