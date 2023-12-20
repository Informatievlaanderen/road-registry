namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class RoadSegmentChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentChangeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var identifiers = new Dictionary<RoadSegmentId, RecordNumber>();
            var moved = records.MoveNext();
            if (moved)
            {
                while (moved)
                {
                    var record = records.Current;
                    if (record != null)
                    {
                        var recordContext = entry
                            .AtDbaseRecord(records.CurrentRecordNumber)
                            .WithIdentifier(nameof(RoadSegmentChangeDbaseRecord.WS_OIDN), record.WS_OIDN.GetValue());

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
                            else if (!recordType.IsAnyOf(RecordType.Identical, RecordType.Added, RecordType.Modified, RecordType.Removed))
                            {
                                problems += recordContext.RecordTypeNotSupported(record.RECORDTYPE.Value, RecordType.Identical.Translation.Identifier, RecordType.Added.Translation.Identifier, RecordType.Modified.Translation.Identifier, RecordType.Removed.Translation.Identifier);
                            }
                        }

                        if (record.WS_OIDN.HasValue)
                        {
                            if (record.WS_OIDN.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                            else if (!RoadSegmentId.Accepts(record.WS_OIDN.Value))
                            {
                                problems += recordContext.RoadSegmentIdOutOfRange(record.WS_OIDN.Value);
                            }
                            else
                            {
                                if (recordType != RecordType.Added)
                                {
                                    var identifier = new RoadSegmentId(record.WS_OIDN.Value);
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

                                if (recordType == RecordType.Identical)
                                {
                                    context = context.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                }
                                else if (recordType == RecordType.Added)
                                {
                                    if (recordType == RecordType.Added && record.EVENTIDN.HasValue && record.EVENTIDN.Value != 0)
                                    {
                                        context = context.WithAddedRoadSegment(new RoadSegmentId(record.EVENTIDN.Value));
                                    }
                                    else
                                    {
                                        context = context.WithAddedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                    }
                                }
                                else if (recordType == RecordType.Modified)
                                {
                                    context = context.WithModifiedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                }
                                else if (recordType == RecordType.Removed)
                                {
                                    context = context.WithRemovedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                }
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WS_OIDN.Field);
                        }

                        problems += recordContext.Validate(record);
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
