namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

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
                            encoding,
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
                            encoding,
                            RoadSegmentChangeDbaseRecord.Schema,
                            new RoadSegmentChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTEUROPWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<EuropeanRoadChangeDbaseRecord>(
                            encoding,
                            EuropeanRoadChangeDbaseRecord.Schema,
                            new EuropeanRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTNATIONWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<NationalRoadChangeDbaseRecord>(
                            encoding,
                            NationalRoadChangeDbaseRecord.Schema,
                            new NationalRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTGENUMWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<NumberedRoadChangeDbaseRecord>(
                            encoding,
                            NumberedRoadChangeDbaseRecord.Schema,
                            new NumberedRoadChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTRIJSTROKEN_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentLaneChangeDbaseRecord>(
                            encoding,
                            RoadSegmentLaneChangeDbaseRecord.Schema,
                            new RoadSegmentLaneChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTWEGBREEDTE_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentWidthChangeDbaseRecord>(
                            encoding,
                            RoadSegmentWidthChangeDbaseRecord.Schema,
                            new RoadSegmentWidthChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "ATTWEGVERHARDING_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding,
                            RoadSegmentSurfaceChangeDbaseRecord.Schema,
                            new RoadSegmentSurfaceChangeDbaseRecordsValidator()
                        )
                    },
                    {
                        "RLTOGKRUISING_ALL.DBF",
                        new ZipArchiveDbaseEntryValidator<GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding,
                            GradeSeparatedJunctionChangeDbaseRecord.Schema,
                            new GradeSeparatedJunctionChangeDbaseRecordsValidator()
                        )
                    }
                };
        }

        public ZipArchiveErrors Validate(ZipArchive archive)
        {
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            var errors = ZipArchiveErrors.None;

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
                    errors = errors.CombineWith(validator.Validate(archive.GetEntry(file)));
                }
            }

            return errors;
        }
    }
}
