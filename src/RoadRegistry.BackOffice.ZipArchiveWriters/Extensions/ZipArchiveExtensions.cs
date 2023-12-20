namespace RoadRegistry.BackOffice.ZipArchiveWriters.Extensions;

using System.IO.Compression;
using System.Text;
using Extracts;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using ShapeFile;

internal static class ZipArchiveExtensions
{
    public static Task<ZipArchive> CreateCpgEntry(this ZipArchive archive, ExtractFileName fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        return CreateCpgEntry(archive, fileName.ToCpgFileName(), encoding, cancellationToken);
    }

    public static async Task<ZipArchive> CreateCpgEntry(this ZipArchive archive, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        var cpgEntry = archive.CreateEntry(fileName);
        await using var cpgEntryStream = cpgEntry.Open();

        var streamWriter = new StreamWriter(cpgEntryStream);
        await streamWriter.WriteAsync(encoding.CodePage.ToString());
        await streamWriter.FlushAsync();

        await cpgEntryStream.FlushAsync(cancellationToken);

        return archive;
    }

    public static async Task<ZipArchive> CreateShapeEntry(this ZipArchive archive, ExtractFileName fileName, Encoding encoding, IEnumerable<IFeature> features, GeometryFactory geometryFactory, CancellationToken cancellationToken)
    {
        var shpStream = new MemoryStream();
        var shxStream = new MemoryStream();

        var writer = new ZipArchiveShapeFileWriter(encoding);
        writer.Write(shpStream, shxStream, features, geometryFactory);

        await CopyToEntry(shpStream, archive.CreateEntry(fileName.ToShapeFileName()), cancellationToken);
        await CopyToEntry(shxStream, archive.CreateEntry(fileName.ToShapeIndexFileName()), cancellationToken);

        return archive;
    }

    private static async Task CopyToEntry(Stream stream, ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        await using var entryStream = entry.Open();
        
        stream.Position = 0;
        await stream.CopyToAsync(entryStream, cancellationToken);

        await entryStream.FlushAsync(cancellationToken);
    }
}
