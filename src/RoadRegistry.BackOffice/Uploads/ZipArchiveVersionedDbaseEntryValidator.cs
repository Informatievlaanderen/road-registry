namespace RoadRegistry.BackOffice.Uploads
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    public class ZipArchiveVersionedDbaseEntryValidator : IZipArchiveEntryValidator
    {
        private readonly Encoding _encoding;
        private readonly DbaseFileHeaderReadBehavior _readBehavior;
        private readonly IReadOnlyDictionary<DbaseSchema, IZipArchiveEntryValidator> _versionedValidators;

        public ZipArchiveVersionedDbaseEntryValidator(
            Encoding encoding,
            DbaseFileHeaderReadBehavior readBehavior,
            IReadOnlyDictionary<DbaseSchema, IZipArchiveEntryValidator> versionedValidators)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _readBehavior = readBehavior ?? throw new ArgumentNullException(nameof(readBehavior));
            _versionedValidators = versionedValidators ?? throw new ArgumentNullException(nameof(versionedValidators));
        }

        public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = ZipArchiveProblems.None;
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, _encoding))
            {
                DbaseFileHeader header = null;
                try
                {
                    header = DbaseFileHeader.Read(reader, _readBehavior);
                }
                catch (Exception exception)
                {
                    problems += entry.HasDbaseHeaderFormatError(exception);
                }

                if (_versionedValidators.ContainsKey(header.Schema))
                {
                    var validator = _versionedValidators.Single(v => v.Key == header.Schema).Value;
                    (problems, context) = validator.Validate(entry, context);
                }
            }

            return (problems, context);
        }
    }
}
