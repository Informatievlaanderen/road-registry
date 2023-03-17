namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Uploads;

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

    public ZipArchiveAfterFeatureCompareValidator(Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        _validators =
            new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "WEGKNOOP_ALL.SHP",
                    new ZipArchiveShapeEntryValidator(
                        encoding,
                        new RoadNodeChangeShapeRecordsValidator()
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
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.RoadNodeChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.RoadNodeChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.RoadNodeChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.RoadNodeChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.RoadNodeChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.RoadNodeChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "WEGSEGMENT_ALL.SHP",
                    new ZipArchiveShapeEntryValidator(
                        encoding,
                        new RoadSegmentChangeShapeRecordsValidator()
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
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.RoadSegmentChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.RoadSegmentChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.RoadSegmentChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.RoadSegmentChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.RoadSegmentChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.RoadSegmentChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTEUROPWEG_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.EuropeanRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.EuropeanRoadChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.EuropeanRoadChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.EuropeanRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.EuropeanRoadChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.EuropeanRoadChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTNATIONWEG_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.NationalRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.NationalRoadChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.NationalRoadChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.NationalRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.NationalRoadChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.NationalRoadChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTGENUMWEG_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.NumberedRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.NumberedRoadChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.NumberedRoadChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.NumberedRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.NumberedRoadChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.NumberedRoadChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTRIJSTROKEN_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.RoadSegmentLaneChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.RoadSegmentLaneChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.RoadSegmentLaneChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.RoadSegmentLaneChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.RoadSegmentLaneChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.RoadSegmentLaneChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTWEGBREEDTE_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.RoadSegmentWidthChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.RoadSegmentWidthChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.RoadSegmentWidthChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.RoadSegmentWidthChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.RoadSegmentWidthChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.RoadSegmentWidthChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "ATTWEGVERHARDING_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.RoadSegmentSurfaceChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.RoadSegmentSurfaceChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.RoadSegmentSurfaceChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.RoadSegmentSurfaceChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "RLTOGKRUISING_ALL.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.GradeSeparatedJunctionChangeDbaseRecord.Schema,
                            new Uploads.Schema.V2.GradeSeparatedJunctionChangeDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.GradeSeparatedJunctionChangeDbaseRecord.Schema,
                            new Uploads.Schema.V1.GradeSeparatedJunctionChangeDbaseRecordsValidator()
                        )
                    )
                },
                {
                    "TRANSACTIEZONES.DBF",
                    new ZipArchiveVersionedDbaseEntryValidator(
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V2.TransactionZoneDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V2.TransactionZoneDbaseRecord.Schema,
                            new Uploads.Schema.V2.TransactionZoneDbaseRecordsValidator()
                        ),
                        new ZipArchiveDbaseEntryValidator<Uploads.Schema.V1.TransactionZoneDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            Uploads.Schema.V1.TransactionZoneDbaseRecord.Schema,
                            new Uploads.Schema.V1.TransactionZoneDbaseRecordsValidator()
                        )
                    )
                }
            };
    }

    public ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveMetadata metadata)
    {
        if (archive == null)
            throw new ArgumentNullException(nameof(archive));
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

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
