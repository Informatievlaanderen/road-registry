namespace RoadRegistry.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    public class ZipArchiveValidator
    {
        private static readonly string[] RequiredFiles =
        {
            "WEGSEGMENT.SHP",
            "WEGKNOOP.SHP",
            "ATTEUROPWEG.DBF",
            "ATTGENUMWEG.DBF",
            "ATTNATIONWEG.DBF",
            "ATTRIJSTROKEN.DBF",
            "ATTWEGBREEDTE.DBF",
            "ATTWEGVERHARDING.DBF",
            "RLTOGKRUISING.DBF",
            "TRANSACTIEZONES.SHP"
        };

        private readonly Dictionary<string, IZipArchiveEntryValidator> _entryValidators;

        public ZipArchiveValidator(Encoding encoding)
        {
            var roadSegmentShapeRecordValidator = new RoadSegmentShapeRecordValidator();
            var roadNodeShapeRecordValidator = new RoadNodeShapeRecordValidator();
            var europeanRoadDbaseRecordValidator = new EuropeanRoadDbaseRecordValidator();
            _entryValidators =
                new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {
                        "WEGSEGMENT.SHP",
                        new ZipArchiveShapeEntryValidator(
                            encoding,
                            () => roadSegmentShapeRecordValidator
                        )
                    },
                    {
                        "WEGKNOOP.SHP",
                        new ZipArchiveShapeEntryValidator(
                            encoding,
                            () => roadNodeShapeRecordValidator
                        )
                    },
                    {
                        "ATTEUROPWEG.DBF",
                        new ZipArchiveDbaseEntryValidator(
                            encoding,
                            new EuropeanRoadComparisonDbaseSchema(),
                            () => europeanRoadDbaseRecordValidator
                        )
                    }
                };
        }

        public ZipArchiveErrors Validate(ZipArchive archive)
        {
            var errors = ZipArchiveErrors.None;

            // Report all missing required files

            var missingRequiredFiles = new HashSet<string>(
                RequiredFiles,
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
                    RequiredFiles,
                    StringComparer.InvariantCultureIgnoreCase
                )
            );

            foreach (var file in availableRequiredFiles)
            {
                if (_entryValidators.TryGetValue(file, out var validator))
                {
                    errors = errors.CombineWith(validator.Validate(archive.GetEntry(file)));
                }
            }

            return errors;
        }
    }
}
