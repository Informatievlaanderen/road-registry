namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.IO.Compression;
using System.Linq;

public static class ZipArchiveExtensions
{
    public static ZipArchiveEntry FindEntry(this ZipArchive archive, string fileName)
    {
        var entries = archive.Entries
            .Where(x => x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        if (entries.Count == 1)
        {
            return entries.Single();
        }

        return null;
    }
}
