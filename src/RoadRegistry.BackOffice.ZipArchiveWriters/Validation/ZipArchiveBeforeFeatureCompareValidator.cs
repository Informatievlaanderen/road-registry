namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema.Extracts;
using Uploads;

/// <summary>
///     BEFORE FEATURE COMPARE
/// </summary>
public class ZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
{
    private static readonly string[] ValidationOrder =
    {
        "TRANSACTIEZONES.DBF",
        "WEGKNOOP.DBF",
        "WEGKNOOP.SHP",
        "WEGSEGMENT.DBF",
        "ATTRIJSTROKEN.DBF",
        "ATTWEGBREEDTE.DBF",
        "ATTWEGVERHARDING.DBF",
        "WEGSEGMENT.SHP",
        "ATTEUROPWEG.DBF",
        "ATTNATIONWEG.DBF",
        "ATTGENUMWEG.DBF",
        "RLTOGKRUISING.DBF"
    };

    private readonly Dictionary<string, IZipArchiveEntryValidator> _validators;

    public ZipArchiveBeforeFeatureCompareValidator(Encoding encoding)
    {
        if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

        _validators =
            new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "WEGKNOOP.SHP",
                    new ZipArchiveShapeEntryValidator(
                        encoding,
                        new RoadNodeChangeShapeRecordsValidator()
                    )
                },
                {
                    "WEGKNOOP.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadNodes.RoadNodeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadNodes.RoadNodeDbaseRecord.Schema,
                            new RoadNodeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadNodeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadNodeDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadNodeDbaseRecordsValidator()
                        ))
                },
                {
                    "WEGSEGMENT.SHP",
                    new ZipArchiveShapeEntryValidator(
                        encoding,
                        new RoadSegmentChangeShapeRecordsValidator()
                    )
                },
                {
                    "WEGSEGMENT.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadSegments.RoadSegmentDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadSegments.RoadSegmentDbaseRecord.Schema,
                            new RoadSegmentDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadSegmentDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadSegmentDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadSegmentDbaseRecordsValidator()
                        ))
                },
                {
                    "ATTEUROPWEG.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadSegments.RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadSegments.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema,
                            new RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator()
                        ))
                },
                {
                    "ATTNATIONWEG.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadSegments.RoadSegmentNationalRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadSegments.RoadSegmentNationalRoadAttributeDbaseRecord.Schema,
                            new RoadSegmentNationalRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadSegmentNationalRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadSegmentNationalRoadAttributeDbaseRecordsValidator()
                        ))
                },
                {
                    "ATTGENUMWEG.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadSegments.RoadSegmentNumberedRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadSegments.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema,
                            new RoadSegmentNumberedRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadSegmentNumberedRoadAttributeDbaseRecordsValidator()
                        ))
                },
                {
                    "ATTRIJSTROKEN.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadSegments.RoadSegmentLaneAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadSegments.RoadSegmentLaneAttributeDbaseRecord.Schema,
                            new RoadSegmentLaneAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadSegmentLaneAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadSegmentLaneAttributeDbaseRecordsValidator()
                        ))
                },
                {
                    "ATTWEGBREEDTE.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadSegments.RoadSegmentWidthAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadSegments.RoadSegmentWidthAttributeDbaseRecord.Schema,
                            new RoadSegmentWidthAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadSegmentWidthAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadSegmentWidthAttributeDbaseRecordsValidator()
                        ))
                },
                {
                    "ATTWEGVERHARDING.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.RoadSegments.RoadSegmentSurfaceAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.RoadSegments.RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                            new RoadSegmentSurfaceAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.RoadSegmentSurfaceAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                            new Uploads.Basic.Validation.RoadSegmentSurfaceAttributeDbaseRecordsValidator()
                        ))
                },
                {
                    "RLTOGKRUISING.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Dbase.GradeSeparatedJuntions.GradeSeparatedJunctionDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Dbase.GradeSeparatedJuntions.GradeSeparatedJunctionDbaseRecord.Schema,
                            new GradeSeparatedJunctionDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Basic.Schema.GradeSeparatedJunctionDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Basic.Schema.GradeSeparatedJunctionDbaseRecord.Schema,
                            new Uploads.Basic.Validation.GradeSeparatedJunctionDbaseRecordsValidator()
                        ))
                },
                {
                    "TRANSACTIEZONES.DBF",
                    new ZipArchiveDbaseEntryValidator<TransactionZoneDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        TransactionZoneDbaseRecord.Schema,
                        new TransactionZoneDbaseRecordsValidator())
                }
            };
    }

    public ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(metadata);

        var problems = ZipArchiveProblems.None;

        // Report all missing required files
        var missingRequiredFiles = new HashSet<string>(
            _validators.Keys,
            StringComparer.InvariantCultureIgnoreCase
        );
        missingRequiredFiles.ExceptWith(
            new HashSet<string>(
                archive.Entries.Select(entry => entry.FullName),
                StringComparer.InvariantCultureIgnoreCase
            )
        );
        problems = missingRequiredFiles.Aggregate(
            problems,
            (current, file) => current.RequiredFileMissing(file));

        // Validate all required files (if a validator was registered for it)

        if (missingRequiredFiles.Count == 0)
        {
            var requiredFiles = new HashSet<string>(
                archive.Entries.Select(entry => entry.FullName),
                StringComparer.InvariantCultureIgnoreCase
            );
            requiredFiles.IntersectWith(
                new HashSet<string>(
                    _validators.Keys,
                    StringComparer.InvariantCultureIgnoreCase
                )
            );

            var context = ZipArchiveValidationContext.Empty.WithZipArchiveMetadata(metadata);

            foreach (var file in
                     requiredFiles
                         .OrderBy(file => Array.IndexOf(ValidationOrder, file.ToUpperInvariant())))
                if (_validators.TryGetValue(file, out var validator))
                {
                    var (fileProblems, fileContext) = validator.Validate(archive.GetEntry(file), context);
                    problems = problems.AddRange(fileProblems);
                    context = fileContext;
                }
        }

        return problems;
    }
}
