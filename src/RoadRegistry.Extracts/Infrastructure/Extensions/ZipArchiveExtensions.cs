namespace RoadRegistry.Extracts.Infrastructure.Extensions;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class ZipArchiveExtensions
{
    public static ZipArchiveEntry? FindEntry(this ZipArchive archive, string fileName)
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

    public static async Task CopyFrom(this ZipArchiveEntry entry, MemoryStream stream, CancellationToken cancellationToken)
    {
        await using var entryStream = entry.Open();

        stream.Position = 0;
        await stream.CopyToAsync(entryStream, cancellationToken);

        await entryStream.FlushAsync(cancellationToken);
    }
}
