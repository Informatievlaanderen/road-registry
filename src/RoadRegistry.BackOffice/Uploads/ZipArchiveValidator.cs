namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
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
                        "WEGKNOOP_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadNodeChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
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
                        "WEGSEGMENT_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            RoadSegmentChangeDbaseRecord.Schema,
                            new RoadSegmentChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTEUROPWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<EuropeanRoadChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            EuropeanRoadChangeDbaseRecord.Schema,
                            new EuropeanRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTNATIONWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<NationalRoadChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            NationalRoadChangeDbaseRecord.Schema,
                            new NationalRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTGENUMWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<NumberedRoadChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            NumberedRoadChangeDbaseRecord.Schema,
                            new NumberedRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTRIJSTROKEN_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentLaneChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            RoadSegmentLaneChangeDbaseRecord.Schema,
                            new RoadSegmentLaneChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTWEGBREEDTE_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentWidthChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            RoadSegmentWidthChangeDbaseRecord.Schema,
                            new RoadSegmentWidthChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTWEGVERHARDING_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            RoadSegmentSurfaceChangeDbaseRecord.Schema,
                            new RoadSegmentSurfaceChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "RLTOGKRUISING_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
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

        public ZipArchiveProblems Validate(ZipArchive archive)
        {
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            var errors = ZipArchiveProblems.None;

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
            errors = missingRequiredFiles.Aggregate(
                errors,
                (current, file) => current.RequiredFileMissing(file));


            // Validate all available required files (if a validator was registered for it)

            var availableRequiredFiles = new HashSet<string>(
                archive.Entries.Select(entry => entry.FullName),
                StringComparer.InvariantCultureIgnoreCase
            );
            availableRequiredFiles.IntersectWith(
                new HashSet<string>(
                    _validators.Keys,
                    StringComparer.InvariantCultureIgnoreCase
                )
            );

            foreach (var file in availableRequiredFiles)
            {
                if (_validators.TryGetValue(file, out var validator))
                {
                    errors = errors.AddRange(validator.Validate(archive.GetEntry(file)));
                }
            }

            return errors;
        }
    }
}
