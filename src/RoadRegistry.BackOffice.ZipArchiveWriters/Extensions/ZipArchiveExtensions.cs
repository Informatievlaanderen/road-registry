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
        await streamWriter.FlushAsync(cancellationToken);

        await cpgEntryStream.FlushAsync(cancellationToken);

        return archive;
    }

    public static async Task<ZipArchive> CreateProjectionEntry(this ZipArchive archive, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        const string staticFileContents = """PROJCS["Belge_Lambert_1972",GEOGCS["GCS_Belge_1972",DATUM["D_Belge_1972",SPHEROID["International_1924",6378388.0,297.0]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],PROJECTION["Lambert_Conformal_Conic"],PARAMETER["False_Easting",150000.01256],PARAMETER["False_Northing",5400088.4378],PARAMETER["Central_Meridian",4.367486666666666],PARAMETER["Standard_Parallel_1",49.8333339],PARAMETER["Standard_Parallel_2",51.16666723333333],PARAMETER["Latitude_Of_Origin",90.0],UNIT["Meter",1.0]]""";

        var prjEntry = archive.CreateEntry(fileName);
        await using (var prjEntryStream = prjEntry.Open())
        await using (var prjEntryStreamWriter = new StreamWriter(prjEntryStream, encoding))
        {
            await prjEntryStreamWriter.WriteAsync(staticFileContents);
            await prjEntryStreamWriter.FlushAsync(cancellationToken);
        }

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
