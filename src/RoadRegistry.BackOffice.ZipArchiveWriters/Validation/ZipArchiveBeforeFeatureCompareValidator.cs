namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase.GradeSeparatedJuntions;
using Dbase.RoadNodes;
using Dbase.RoadSegments;
using Editor.Schema.Extracts;
using Uploads;

/// <summary>
///     PRE FEATURE COMPARE
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
                    new ZipArchiveDbaseEntryValidator<RoadNodeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadNodeDbaseRecord.Schema,
                        new RoadNodeDbaseRecordsValidator()
                    )
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
                    new ZipArchiveDbaseEntryValidator<RoadSegmentDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentDbaseRecord.Schema,
                        new RoadSegmentDbaseRecordsValidator()
                    )
                },
                {
                    "ATTEUROPWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema,
                        new RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTNATIONWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentNationalRoadAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentNationalRoadAttributeDbaseRecord.Schema,
                        new RoadSegmentNationalRoadAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTGENUMWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentNumberedRoadAttributeDbaseRecord.Schema,
                        new RoadSegmentNumberedRoadAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTRIJSTROKEN.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentLaneAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentLaneAttributeDbaseRecord.Schema,
                        new RoadSegmentLaneAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTWEGBREEDTE.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentWidthAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentWidthAttributeDbaseRecord.Schema,
                        new RoadSegmentWidthAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTWEGVERHARDING.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceAttributeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                        new RoadSegmentSurfaceAttributeDbaseRecordsValidator()
                    )
                },
                {
                    "RLTOGKRUISING.DBF",
                    new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        GradeSeparatedJunctionDbaseRecord.Schema,
                        new GradeSeparatedJunctionDbaseRecordsValidator()
                    )
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
