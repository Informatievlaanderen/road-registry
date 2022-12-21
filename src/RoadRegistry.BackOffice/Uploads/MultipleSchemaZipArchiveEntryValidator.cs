namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO.Compression;
using System.Linq;

public class MultipleSchemaZipArchiveEntryValidator : IZipArchiveEntryValidator
{
    private readonly IZipArchiveEntryValidator[] _validators;

    public MultipleSchemaZipArchiveEntryValidator(params IZipArchiveEntryValidator[] validators)
    {
        if (validators.Length == 0)
        {
            throw new ArgumentNullException(nameof(validators));
        }

        _validators = validators;
    }

    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context)
    {
        (ZipArchiveProblems, ZipArchiveValidationContext)? result = null;

        foreach (var validator in _validators)
        {
            result = validator.Validate(entry, context);

            var hasDbaseSchemaMismatch = result.Value.Item1.Any(x => x.Reason == nameof(DbaseFileProblems.HasDbaseSchemaMismatch));
            if (!hasDbaseSchemaMismatch)
            {
                return result.Value;
            }
        }

        if (result == null)
        {
            throw new InvalidOperationException("No validator generated a result");
        }

        return result.Value;
    }
}
