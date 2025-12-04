namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using RoadRegistry.Extensions;

public class ZipArchiveProjectionFormatEntryValidator : IZipArchiveEntryValidator
{
    private readonly Encoding _encoding;

    public ZipArchiveProjectionFormatEntryValidator(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public ZipArchiveProblems Validate(ZipArchiveEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var problems = ZipArchiveProblems.None;

        using (var stream = entry.Open())
        using (var reader = new StreamReader(stream, _encoding))
        {
            var projectionFormat = ProjectionFormat.Read(reader);
            if (!projectionFormat.IsBelgeLambert1972())
            {
                problems += entry.ProjectionFormatInvalid();
            }
        }

        return problems;
    }
}
