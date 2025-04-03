namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using NetTopologySuite.IO.Esri.Shapefiles.Writers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.ZipArchiveWriters.Extensions;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class DbaseRecordWriter
{
    private readonly Encoding _encoding;

    public DbaseRecordWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public Task WriteToArchive(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        DbaseSchema dbaseSchema,
        IEnumerable<DbaseRecord> dbaseRecords,
        CancellationToken cancellationToken)
    {
        return WriteToArchive(archive, fileName.ToDbaseFileName(featureType), dbaseSchema, dbaseRecords, cancellationToken);
    }

    public async Task WriteToArchive(
        ZipArchive archive,
        string fileName,
        DbaseSchema dbaseSchema,
        IEnumerable<DbaseRecord> dbaseRecords,
        CancellationToken cancellationToken)
    {
        var dbfFields = dbaseSchema.ToDbfFields();

        var dbfStream = new MemoryStream();
        using (var dbfWriter = new DbfWriter(dbfStream, dbfFields, _encoding))
        {
            foreach (var dbaseRecord in dbaseRecords)
            {
                var attributes = dbaseRecord
                    .ToAttributesTable()
                    .ToDictionary(x => x.Key, x => x.Value);
                dbfWriter.Write(attributes);
            }
        }

        var dbfEntry = archive.CreateEntry(fileName);
        await CopyToEntry(dbfStream, dbfEntry, cancellationToken);
    }

    public async Task WriteToArchive(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        DbaseSchema dbaseSchema,
        ShapeType shapeType,
        IEnumerable<(DbaseRecord, Geometry)> dbaseRecords,
        CancellationToken cancellationToken)
    {
        var dbfFields = dbaseSchema.ToDbfFields();

        var features = dbaseRecords
            .Select(record => record.Item1.ToFeature(record.Item2))
            .ToList();

        await WriteToArchive(archive, fileName, featureType, dbfFields, shapeType, features, _encoding, cancellationToken);
    }

    private static async Task WriteToArchive(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        DbfField[] dbfFields,
        ShapeType shapeType,
        ICollection<IFeature> features,
        Encoding encoding,
        CancellationToken cancellationToken)
    {
        const string projection = """PROJCS["Belge_Lambert_1972",GEOGCS["GCS_Belge_1972",DATUM["D_Belge_1972",SPHEROID["International_1924",6378388.0,297.0]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],PROJECTION["Lambert_Conformal_Conic"],PARAMETER["False_Easting",150000.01256],PARAMETER["False_Northing",5400088.4378],PARAMETER["Central_Meridian",4.367486666666666],PARAMETER["Standard_Parallel_1",49.8333339],PARAMETER["Standard_Parallel_2",51.16666723333333],PARAMETER["Latitude_Of_Origin",90.0],UNIT["Meter",1.0]]""";

        var shpStream = new MemoryStream();
        var shxStream = new MemoryStream();
        var dbfStream = new MemoryStream();
        var prjStream = new MemoryStream();

        var options = new ShapefileWriterOptions(shapeType, dbfFields)
        {
            Projection = projection,
            Encoding = encoding
        };

        using (var shpWriter = OpenWrite(shpStream, shxStream, dbfStream, prjStream, options))
        {
            shpWriter.Write(features);
        }

        var shpEntry = archive.CreateEntry(fileName.ToShapeFileName(featureType));
        await CopyToEntry(shpStream, shpEntry, cancellationToken);

        var shxEntry = archive.CreateEntry(fileName.ToShapeIndexFileName(featureType));
        await CopyToEntry(shxStream, shxEntry, cancellationToken);

        var dbfEntry = archive.CreateEntry(fileName.ToDbaseFileName(featureType));
        await CopyToEntry(dbfStream, dbfEntry, cancellationToken);

        var prjEntry = archive.CreateEntry(fileName.ToProjectionFileName(featureType));
        await CopyToEntry(prjStream, prjEntry, cancellationToken);

        await archive.CreateCpgEntry(fileName.ToCpgFileName(featureType), encoding, cancellationToken);
    }

    private static async Task CopyToEntry(MemoryStream stream, ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        await using var entryStream = entry.Open();

        stream.Position = 0;
        await stream.CopyToAsync(entryStream, cancellationToken);

        await entryStream.FlushAsync(cancellationToken);
    }

    private static ShapefileWriter OpenWrite(Stream shpStream, Stream shxStream, Stream dbfStream, Stream prjStream, ShapefileWriterOptions options)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        if (options.ShapeType.IsPoint())
        {
            return new ShapefilePointWriter(shpStream, shxStream, dbfStream, prjStream, options);
        }
        if (options.ShapeType.IsMultiPoint())
        {
            return new ShapefileMultiPointWriter(shpStream, shxStream, dbfStream, prjStream, options);
        }
        if (options.ShapeType.IsPolyLine())
        {
            return new ShapefilePolyLineWriter(shpStream, shxStream, dbfStream, prjStream, options);
        }
        if (options.ShapeType.IsPolygon())
        {
            return new ShapefilePolygonWriter(shpStream, shxStream, dbfStream, prjStream, options);
        }

        throw new ShapefileException("Unsupported shapefile type: " + options.ShapeType);
    }
}
