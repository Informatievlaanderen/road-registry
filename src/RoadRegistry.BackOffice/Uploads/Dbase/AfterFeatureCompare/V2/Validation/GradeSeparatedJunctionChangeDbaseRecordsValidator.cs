namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class GradeSeparatedJunctionChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<GradeSeparatedJunctionChangeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var identifiers = new Dictionary<GradeSeparatedJunctionId, RecordNumber>();
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

                        if (record.OK_OIDN.HasValue)
                        {
                            if (record.OK_OIDN.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                            else
                            {
                                if (recordType != RecordType.Added)
                                {
                                    var identifier = new GradeSeparatedJunctionId(record.OK_OIDN.Value);
                                    if (identifiers.TryGetValue(identifier, out var takenByRecordNumber))
                                    {
                                        problems += recordContext.IdentifierNotUnique(
                                            identifier,
                                            takenByRecordNumber
                                        );
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
                            problems += recordContext.RequiredFieldIsNull(record.OK_OIDN.Field);
                        }

                        if (record.TYPE.HasValue)
                        {
                            if (!GradeSeparatedJunctionType.ByIdentifier.ContainsKey(record.TYPE.Value))
                            {
                                problems += recordContext.GradeSeparatedJunctionTypeMismatch(record.TYPE.Value);
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                        }

                        if (!record.BO_WS_OIDN.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.BO_WS_OIDN.Field);
                        }
                        else if (!RoadSegmentId.Accepts(record.BO_WS_OIDN.Value))
                        {
                            problems += recordContext.UpperRoadSegmentIdOutOfRange(record.BO_WS_OIDN.Value);
                        }

                        if (!record.ON_WS_OIDN.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.ON_WS_OIDN.Field);
                        }
                        else if (!RoadSegmentId.Accepts(record.ON_WS_OIDN.Value))
                        {
                            problems += recordContext.LowerRoadSegmentIdOutOfRange(record.ON_WS_OIDN.Value);
                        }

                        moved = records.MoveNext();
                    }
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
