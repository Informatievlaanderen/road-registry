namespace RoadRegistry.Product.PublishHost.Infrastructure;

using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class FileExtensions
{
    public static async Task AddToZipArchive(
        this ZipArchive archive,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
        await using var entryStream = entry.Open();
        await entryStream.WriteAsync(content, cancellationToken);
        await entryStream.FlushAsync(cancellationToken);
    }

    public static async Task AddToZipArchive(
        this ZipArchive archive,
        string fileName,
        string content,
        CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        await archive.AddToZipArchive(fileName, bytes, cancellationToken);
    }
}
