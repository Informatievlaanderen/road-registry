namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Be.Vlaanderen.Basisregisters.Shaperon;
using System.IO.Compression;
using Uploads;
using Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using Uploads.Dbase.AfterFeatureCompare.V2.Validation;

/// <summary>
///     POST FEATURE COMPARE
/// </summary>
public class ZipArchiveAfterFeatureCompareValidator : IZipArchiveAfterFeatureCompareValidator
{
    private static readonly string[] ValidationOrder =
    {
        "TRANSACTIEZONES.DBF",
        "WEGKNOOP_ALL.DBF",
        "WEGKNOOP_ALL.SHP",
        "WEGKNOOP_ALL.PRJ",
        "WEGSEGMENT_ALL.DBF",
        "ATTRIJSTROKEN_ALL.DBF",
        "ATTWEGBREEDTE_ALL.DBF",
        "ATTWEGVERHARDING_ALL.DBF",
        "WEGSEGMENT_ALL.SHP",
        "WEGSEGMENT_ALL.PRJ",
        "ATTEUROPWEG_ALL.DBF",
        "ATTNATIONWEG_ALL.DBF",
        "ATTGENUMWEG_ALL.DBF",
        "RLTOGKRUISING_ALL.DBF"
    };

    private readonly Dictionary<string, IZipArchiveEntryValidator> _validators;

    public ZipArchiveAfterFeatureCompareValidator(FileEncoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        _validators =
            new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "WEGKNOOP_ALL.SHP",
                    new ZipArchiveShapeEntryValidator(
                        new RoadNodeChangeShapeRecordValidator()
                    )
                },
                {
                    "WEGKNOOP_ALL.PRJ",
                    new ZipArchiveProjectionFormatEntryValidator(
                        encoding
                    )
                },
                {
                    "WEGKNOOP_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadNodeChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadNodeChangeDbaseRecord.Schema,
                            new RoadNodeChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadNodeChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadNodeChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.RoadNodeChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "WEGSEGMENT_ALL.SHP",
                    new ZipArchiveShapeEntryValidator(
                        new RoadSegmentChangeShapeRecordValidator()
                    )
                },
                {
                    "WEGSEGMENT_ALL.PRJ",
                    new ZipArchiveProjectionFormatEntryValidator(
                        encoding
                    )
                },
                {
                    "WEGSEGMENT_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentChangeDbaseRecord.Schema,
                            new RoadSegmentChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTEUROPWEG_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<EuropeanRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            EuropeanRoadChangeDbaseRecord.Schema,
                            new EuropeanRoadChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.EuropeanRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.EuropeanRoadChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.EuropeanRoadChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTNATIONWEG_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<NationalRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            NationalRoadChangeDbaseRecord.Schema,
                            new NationalRoadChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.NationalRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.NationalRoadChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.NationalRoadChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTGENUMWEG_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<NumberedRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            NumberedRoadChangeDbaseRecord.Schema,
                            new NumberedRoadChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.NumberedRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.NumberedRoadChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.NumberedRoadChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTRIJSTROKEN_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentLaneChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentLaneChangeDbaseRecord.Schema,
                            new RoadSegmentLaneChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentLaneChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentLaneChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentLaneChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTWEGBREEDTE_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentWidthChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentWidthChangeDbaseRecord.Schema,
                            new RoadSegmentWidthChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentWidthChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentWidthChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentWidthChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTWEGVERHARDING_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentSurfaceChangeDbaseRecord.Schema,
                            new RoadSegmentSurfaceChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentSurfaceChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentSurfaceChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "RLTOGKRUISING_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            GradeSeparatedJunctionChangeDbaseRecord.Schema,
                            new GradeSeparatedJunctionChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.GradeSeparatedJunctionChangeDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.GradeSeparatedJunctionChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "TRANSACTIEZONES.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<TransactionZoneDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            TransactionZoneDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V2.Validation.TransactionZoneDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Dbase.AfterFeatureCompare.V1.Schema.TransactionZoneDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Dbase.AfterFeatureCompare.V1.Schema.TransactionZoneDbaseRecord.Schema,
                            new Uploads.Dbase.AfterFeatureCompare.V1.Validation.TransactionZoneDbaseRecordsValidator()
                        )
                    )
                }
            };
    }

    public ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveValidatorContext context)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(context);

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

            var validationContext = ZipArchiveValidationContext.Empty
                .WithZipArchiveMetadata(context.ZipArchiveMetadata);

            foreach (var file in
                     requiredFiles
                         .OrderBy(file => Array.IndexOf(ValidationOrder, file.ToUpperInvariant())))
                if (_validators.TryGetValue(file, out var validator))
                {
                    var (fileProblems, fileContext) = validator.Validate(archive.GetEntry(file), validationContext);
                    problems = problems.AddRange(fileProblems);
                    validationContext = fileContext;
                }
        }

        return problems;
    }
}
