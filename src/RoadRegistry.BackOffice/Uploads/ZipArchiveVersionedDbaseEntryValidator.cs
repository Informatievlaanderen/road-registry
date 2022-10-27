namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class ZipArchiveVersionedDbaseEntryValidator : IZipArchiveEntryValidator
{
    private readonly IEnumerable<IZipArchiveDbaseEntryValidator> _versionedValidators;

    public ZipArchiveVersionedDbaseEntryValidator(params IZipArchiveDbaseEntryValidator[] validators)
    {
        _versionedValidators = validators;
    }

    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry,
        ZipArchiveValidationContext context)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        if (context == null) throw new ArgumentNullException(nameof(context));

        IZipArchiveDbaseEntryValidator validator = null;

        var problems = ZipArchiveProblems.None;
        foreach (var validatorCandidate in _versionedValidators)
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, validatorCandidate.Encoding))
            {
                var header = DbaseFileHeader.Read(reader, validatorCandidate.HeaderReadBehavior);
                if (!header.Schema.Equals(validatorCandidate.Schema)) continue;

                validator = validatorCandidate;
                break;
            }

        if (validator != null) (problems, context) = validator.Validate(entry, context);

        return (problems, context);
    }
}