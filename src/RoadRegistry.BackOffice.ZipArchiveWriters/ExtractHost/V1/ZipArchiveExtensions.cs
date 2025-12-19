namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V1;

using System.IO.Compression;
using System.Text;
using BackOffice.Extensions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.ShapeFile.V1;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Extensions;

internal static class ZipArchiveExtensions
{
    public static async Task CreateProjectionEntry(this ZipArchive archive, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        const string staticFileContents = """PROJCS["Belge_Lambert_1972",GEOGCS["GCS_Belge_1972",DATUM["D_Belge_1972",SPHEROID["International_1924",6378388.0,297.0]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],PROJECTION["Lambert_Conformal_Conic"],PARAMETER["False_Easting",150000.01256],PARAMETER["False_Northing",5400088.4378],PARAMETER["Central_Meridian",4.367486666666666],PARAMETER["Standard_Parallel_1",49.8333339],PARAMETER["Standard_Parallel_2",51.16666723333333],PARAMETER["Latitude_Of_Origin",90.0],UNIT["Meter",1.0]]""";

        var prjEntry = archive.CreateEntry(fileName);

        await using var prjEntryStream = prjEntry.Open();
        await using var prjEntryStreamWriter = new StreamWriter(prjEntryStream, encoding);
        await prjEntryStreamWriter.WriteAsync(staticFileContents);

        await prjEntryStreamWriter.FlushAsync(cancellationToken);
    }

    public static async Task CreateShapeEntry(this ZipArchive archive, ExtractFileName fileName, Encoding encoding, IEnumerable<IFeature> features, GeometryFactory geometryFactory, CancellationToken cancellationToken)
    {
        using var shpStream = new MemoryStream();
        using var shxStream = new MemoryStream();

        var writer = new ZipArchiveShapeFileWriter(encoding);
        writer.Write(shpStream, shxStream, features, geometryFactory);

        var shpEntry = archive.CreateEntry(fileName.ToShapeFileName());
        await shpEntry.CopyFrom(shpStream , cancellationToken);

        var shxEntry = archive.CreateEntry(fileName.ToShapeIndexFileName());
        await shxEntry.CopyFrom(shxStream, cancellationToken);
    }
}
