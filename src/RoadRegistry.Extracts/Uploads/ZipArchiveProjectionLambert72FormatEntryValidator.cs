namespace RoadRegistry.Extracts.Uploads;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using RoadRegistry.Extensions;

public class ZipArchiveProjectionLambert72FormatEntryValidator : IZipArchiveEntryValidator
{
    private readonly Encoding _encoding;

    public ZipArchiveProjectionLambert72FormatEntryValidator(Encoding encoding)
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
                problems += entry.ProjectionFormatNotLambert72();
            }
        }

        return problems;
    }
}
