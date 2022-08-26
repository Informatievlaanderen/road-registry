namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class ZipArchiveValidator : IZipArchiveValidator
    {
        private readonly Dictionary<string, IZipArchiveEntryValidator> _validators;

        public ZipArchiveValidator(Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

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
                        new ZipArchiveDbaseEntryValidator<RoadNodeChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadNodeChangeDbaseRecord.Schema,
                            new RoadNodeChangeDbaseRecordsValidator()
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
                        new ZipArchiveDbaseEntryValidator<RoadSegmentChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentChangeDbaseRecord.Schema,
                            new RoadSegmentChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTEUROPWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<EuropeanRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            EuropeanRoadChangeDbaseRecord.Schema,
                            new EuropeanRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTNATIONWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<NationalRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            NationalRoadChangeDbaseRecord.Schema,
                            new NationalRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTGENUMWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<NumberedRoadChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            NumberedRoadChangeDbaseRecord.Schema,
                            new NumberedRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTRIJSTROKEN_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentLaneChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentLaneChangeDbaseRecord.Schema,
                            new RoadSegmentLaneChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTWEGBREEDTE_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentWidthChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentWidthChangeDbaseRecord.Schema,
                            new RoadSegmentWidthChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTWEGVERHARDING_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            RoadSegmentSurfaceChangeDbaseRecord.Schema,
                            new RoadSegmentSurfaceChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "RLTOGKRUISING_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            GradeSeparatedJunctionChangeDbaseRecord.Schema,
                            new GradeSeparatedJunctionChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "TRANSACTIEZONES.DBF",
                        new ZipArchiveVersionedDbaseEntryValidator(
                            new ZipArchiveDbaseEntryValidator<Schema.V1.TransactionZoneDbaseRecord>(
                                encoding, new DbaseFileHeaderReadBehavior(true),
                                Schema.V1.TransactionZoneDbaseRecord.Schema,
                                new Schema.V1.TransactionZoneDbaseRecordsValidator()),
                            new ZipArchiveDbaseEntryValidator<Schema.V2.TransactionZoneDbaseRecord>(
                                encoding, new DbaseFileHeaderReadBehavior(true),
                                Schema.V2.TransactionZoneDbaseRecord.Schema,
                                new Schema.V2.TransactionZoneDbaseRecordsValidator())
                        )
                    }
                };
        }

        private static readonly string[] ValidationOrder = { // MC Hammer Style
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
                {
                    if (_validators.TryGetValue(file, out var validator))
                    {
                        var (fileProblems, fileContext) = validator.Validate(archive.GetEntry(file), context);
                        problems = problems.AddRange(fileProblems);
                        context = fileContext;
                    }
                }
            }

            return problems;
        }
    }
}
