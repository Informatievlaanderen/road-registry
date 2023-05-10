namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase;
using Extracts.Dbase.GradeSeparatedJuntions;
using Extracts.Dbase.RoadNodes;
using Extracts.Dbase.RoadSegments;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Uploads;

public class ZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
{
    private static readonly string[] ValidationOrder =
    {
        "TRANSACTIEZONES.DBF",
        "EWEGKNOOP.DBF",
        "WEGKNOOP.DBF",
        "EWEGKNOOP.SHP",
        "WEGKNOOP.SHP",
        "WEGKNOOP.PRJ",
        "EWEGSEGMENT.DBF",
        "WEGSEGMENT.DBF",
        "EWEGSEGMENT.SHP",
        "WEGSEGMENT.SHP",
        "WEGSEGMENT.PRJ",
        "EATTRIJSTROKEN.DBF",
        "ATTRIJSTROKEN.DBF",
        "EATTWEGBREEDTE.DBF",
        "ATTWEGBREEDTE.DBF",
        "EATTWEGVERHARDING.DBF",
        "ATTWEGVERHARDING.DBF",
        "EATTEUROPWEG.DBF",
        "ATTEUROPWEG.DBF",
        "EATTNATIONWEG.DBF",
        "ATTNATIONWEG.DBF",
        "EATTGENUMWEG.DBF",
        "ATTGENUMWEG.DBF",
        "ERLTOGKRUISING.DBF",
        "RLTOGKRUISING.DBF"
    };

    private readonly Dictionary<string, IZipArchiveEntryValidator> _validators;

    public ZipArchiveBeforeFeatureCompareValidator(FileEncoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        _validators =
            new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "EWEGKNOOP.SHP",
                    new ZipArchiveShapeEntryValidator(
                        new RoadNodeChangeShapeRecordValidator()
                    )
                },
                {
                    "WEGKNOOP.SHP",
                    new ZipArchiveShapeEntryValidator(
                        new RoadNodeChangeShapeRecordValidator()
                    )
                },
                {
                    "IWEGKNOOP.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadNodeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadNodeDbaseRecord.Schema,
                        new RoadNodeDbaseRecordsValidator()
                    )
                },
                {
                    "EWEGKNOOP.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadNodeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadNodeDbaseRecord.Schema,
                        new RoadNodeDbaseRecordsValidator()
                    )
                },
                {
                    "WEGKNOOP.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadNodeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadNodeDbaseRecord.Schema,
                            new RoadNodeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadNodeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadNodeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "WEGKNOOP.PRJ",
                    new ZipArchiveProjectionFormatEntryValidator(
                        encoding
                    )
                },
                {
                    "EWEGSEGMENT.SHP",
                    new ZipArchiveShapeEntryValidator(
                        new RoadSegmentChangeShapeRecordValidator()
                    )
                },
                {
                    "WEGSEGMENT.SHP",
                    new ZipArchiveShapeEntryValidator(
                        new RoadSegmentChangeShapeRecordValidator()
                    )
                },
                {
                    "IWEGSEGMENT.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentDbaseRecord.Schema,
                        new RoadSegmentDbaseRecordsValidator()
                    )
                },
                {
                    "EWEGSEGMENT.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentDbaseRecord.Schema,
                        new RoadSegmentDbaseRecordsValidator()
                    )
                },
                {
                    "WEGSEGMENT.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentDbaseRecord.Schema,
                            new RoadSegmentDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadSegmentDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadSegmentDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "WEGSEGMENT.PRJ",
                    new ZipArchiveProjectionFormatEntryValidator(
                        encoding
                    )
                },
                {
                    "EATTEUROPWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema,
                        new RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTEUROPWEG.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema,
                            new RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "EATTNATIONWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentNationalRoadAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentNationalRoadAttributeDbaseRecord.Schema,
                        new RoadSegmentNationalRoadAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTNATIONWEG.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentNationalRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentNationalRoadAttributeDbaseRecord.Schema,
                            new RoadSegmentNationalRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadSegmentNationalRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadSegmentNationalRoadAttributeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "EATTGENUMWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentNumberedRoadAttributeDbaseRecord.Schema,
                        new RoadSegmentNumberedRoadAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTGENUMWEG.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentNumberedRoadAttributeDbaseRecord.Schema,
                            new RoadSegmentNumberedRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadSegmentNumberedRoadAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadSegmentNumberedRoadAttributeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "EATTRIJSTROKEN.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentLaneAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentLaneAttributeDbaseRecord.Schema,
                        new RoadSegmentLaneAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTRIJSTROKEN.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentLaneAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentLaneAttributeDbaseRecord.Schema,
                            new RoadSegmentLaneAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadSegmentLaneAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadSegmentLaneAttributeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "EATTWEGBREEDTE.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentWidthAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentWidthAttributeDbaseRecord.Schema,
                        new RoadSegmentWidthAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTWEGBREEDTE.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentWidthAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentWidthAttributeDbaseRecord.Schema,
                            new RoadSegmentWidthAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadSegmentWidthAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadSegmentWidthAttributeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "EATTWEGVERHARDING.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                        new RoadSegmentSurfaceAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTWEGVERHARDING.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                            new RoadSegmentSurfaceAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.RoadSegmentSurfaceAttributeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.RoadSegmentSurfaceAttributeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ERLTOGKRUISING.DBF",
                    new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        GradeSeparatedJunctionDbaseRecord.Schema,
                        new GradeSeparatedJunctionDbaseRecordsValidator()
                    )
                },
                {
                    "RLTOGKRUISING.DBF",
                    new MultipleSchemaZipArchiveEntryValidator(
                        new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            GradeSeparatedJunctionDbaseRecord.Schema,
                            new GradeSeparatedJunctionDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V2.Validation.GradeSeparatedJunctionDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.GradeSeparatedJunctionDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.BeforeFeatureCompare.V1.Schema.GradeSeparatedJunctionDbaseRecord.Schema,
                            new Uploads.Dbase.BeforeFeatureCompare.V1.Validation.GradeSeparatedJunctionDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "TRANSACTIEZONES.DBF",
                    new ZipArchiveDbaseEntryValidator<TransactionZoneDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        TransactionZoneDbaseRecord.Schema,
                        new TransactionZoneDbaseRecordsValidator()
                    )
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
