namespace RoadRegistry.BackOffice.ZipArchiveWriters.Extensions;

using System.IO.Compression;
using System.Text;

internal static class ZipArchiveExtensions
{
    public static async Task CreateCpgEntry(this ZipArchive archive, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        var cpgEntry = archive.CreateEntry(fileName);
        await using var cpgEntryStream = cpgEntry.Open();

        var streamWriter = new StreamWriter(cpgEntryStream);
        await streamWriter.WriteAsync(encoding.CodePage.ToString());
        await streamWriter.FlushAsync(cancellationToken);

        await cpgEntryStream.FlushAsync(cancellationToken);
    }
}
