namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.IO.Compression;
using System.Linq;

public static class ZipArchiveExtensions
{
    public static ZipArchiveEntry FindEntry(this ZipArchive archive, string fileName)
    {
        return archive.Entries.SingleOrDefault(x => x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
    }
}
