namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;

public class ZipArchiveFeatureCompareValidator : IZipArchiveValidator
{
    private static readonly string[] ValidationOrder =
    {
        // MC Hammer Style
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

    public ZipArchiveFeatureCompareValidator(Encoding encoding)
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
                    new ZipArchiveDbaseEntryValidator<RoadNodeChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadNodeChangeDbaseRecord.Schema,
                        new RoadNodeChangeDbaseRecordsValidator()
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
                    new ZipArchiveDbaseEntryValidator<RoadSegmentChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentChangeDbaseRecord.Schema,
                        new RoadSegmentChangeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTEUROPWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<EuropeanRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        EuropeanRoadChangeDbaseRecord.Schema,
                        new EuropeanRoadChangeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTNATIONWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<NationalRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        NationalRoadChangeDbaseRecord.Schema,
                        new NationalRoadChangeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTGENUMWEG.DBF",
                    new ZipArchiveDbaseEntryValidator<NumberedRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        NumberedRoadChangeDbaseRecord.Schema,
                        new NumberedRoadChangeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTRIJSTROKEN.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentLaneChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentLaneChangeDbaseRecord.Schema,
                        new RoadSegmentLaneChangeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTWEGBREEDTE.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentWidthChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentWidthChangeDbaseRecord.Schema,
                        new RoadSegmentWidthChangeDbaseRecordsValidator()
                    )
                },
                {
                    "ATTWEGVERHARDING.DBF",
                    new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        RoadSegmentSurfaceChangeDbaseRecord.Schema,
                        new RoadSegmentSurfaceChangeDbaseRecordsValidator()
                    )
                },
                {
                    "RLTOGKRUISING.DBF",
                    new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        GradeSeparatedJunctionChangeDbaseRecord.Schema,
                        new GradeSeparatedJunctionChangeDbaseRecordsValidator()
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
