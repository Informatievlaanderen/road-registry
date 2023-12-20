namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class RoadNodeChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadNodeChangeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var identifiers = new Dictionary<RoadNodeId, (RecordNumber RecordNumber, RecordType RecordType)>();
            var moved = records.MoveNext();
            if (moved)
            {
                while (moved)
                {
                    var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                    var record = records.Current;
                    if (record != null)
                    {
                        if (!record.RECORDTYPE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.RECORDTYPE.Field);
                        }
                        else
                        {
                            if (!RecordType.ByIdentifier.ContainsKey(record.RECORDTYPE.Value))
                            {
                                problems += recordContext.RecordTypeMismatch(record.RECORDTYPE.Value);
                            }
                        }

                        if (record.WEGKNOOPID.HasValue)
                        {
                            if (record.WEGKNOOPID.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                            else
                            {
                                var identifier = new RoadNodeId(record.WEGKNOOPID.Value);

                                if (RecordType.ByIdentifier.TryGetValue(record.RECORDTYPE.Value, out var recordType))
                                {
                                    context = context.WithRoadNode(identifier, recordType);
                                }

                                if (identifiers.TryGetValue(identifier, out var takenBy))
                                {
                                    if (takenBy.RecordType == recordType)
                                        // error
                                    {
                                        problems += recordContext.IdentifierNotUnique(
                                            identifier,
                                            takenBy.RecordNumber
                                        );
                                    }
                                    else
                                        // warning
                                    {
                                        problems += recordContext.IdentifierNotUniqueButAllowed(
                                            identifier,
                                            recordType,
                                            takenBy.RecordNumber,
                                            takenBy.RecordType
                                        );
                                    }
                                }
                                else
                                {
                                    identifiers.Add(identifier, (records.CurrentRecordNumber, recordType));
                                }
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WEGKNOOPID.Field);
                        }

                        if (!record.TYPE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                        }
                        else if (!RoadNodeType.ByIdentifier.ContainsKey(record.TYPE.Value))
                        {
                            problems += recordContext.RoadNodeTypeMismatch(record.TYPE.Value);
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
