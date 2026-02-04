namespace RoadRegistry.Extracts.Infrastructure.ShapeFile;

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;
using Extensions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using NetTopologySuite.IO.Esri.Shapefiles.Writers;
using RoadRegistry.Extensions;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class ShapeFileRecordWriter
{
    private readonly Encoding _encoding;
    private readonly string _projection;

    public ShapeFileRecordWriter(Encoding encoding)
        : this(encoding, 31370)
    {
    }

    public ShapeFileRecordWriter(Encoding encoding, int srid)
    {
        _encoding = encoding.ThrowIfNull();
        _projection = BuildProjection(srid);
    }

    public async Task WriteToArchive(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        ShapeType shapeType,
        DbaseSchema dbaseSchema,
        IEnumerable<(DbaseRecord, Geometry)> records,
        CancellationToken cancellationToken)
    {
        var dbfFields = dbaseSchema.ToDbfFields();

        var features = records
            .Select(record => record.Item1.ToFeature(record.Item2))
            .ToList();

        await WriteToArchive(archive, fileName, featureType, shapeType, dbfFields, features, cancellationToken);
    }

    public async Task WriteToArchive(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        ShapeType shapeType,
        DbfField[] dbfFields,
        ICollection<IFeature> features,
        CancellationToken cancellationToken)
    {
        var shpStream = new MemoryStream();
        var shxStream = new MemoryStream();
        var dbfStream = new MemoryStream();
        var prjStream = new MemoryStream();

        var options = new ShapefileWriterOptions(shapeType, dbfFields)
        {
            Projection = _projection,
            Encoding = _encoding
        };

        using (var shpWriter = OpenWrite(shpStream, shxStream, dbfStream, prjStream, options))
        {
            shpWriter.Write(features);
        }

        var shpEntry = archive.CreateEntry(fileName.ToShapeFileName(featureType));
        await shpEntry.CopyFrom(shpStream, cancellationToken);

        var shxEntry = archive.CreateEntry(fileName.ToShapeIndexFileName(featureType));
        await shxEntry.CopyFrom(shxStream, cancellationToken);

        var dbfEntry = archive.CreateEntry(fileName.ToDbaseFileName(featureType));
        await dbfEntry.CopyFrom(dbfStream, cancellationToken);

        var prjEntry = archive.CreateEntry(fileName.ToProjectionFileName(featureType));
        await prjEntry.CopyFrom(prjStream, cancellationToken);

        await CreateCpgEntry(archive, fileName.ToCpgFileName(featureType), _encoding, cancellationToken);
    }

    private static async Task CreateCpgEntry(ZipArchive archive, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        var cpgEntry = archive.CreateEntry(fileName);
        await using var cpgEntryStream = cpgEntry.Open();

        var streamWriter = new StreamWriter(cpgEntryStream);
        await streamWriter.WriteAsync(encoding.CodePage.ToString());
        await streamWriter.FlushAsync(cancellationToken);

        await cpgEntryStream.FlushAsync(cancellationToken);
    }

    private static ShapefileWriter OpenWrite(Stream shpStream, Stream shxStream, Stream dbfStream, Stream prjStream, ShapefileWriterOptions options)
    {
        options = options.ThrowIfNull();

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

    private static string BuildProjection(int srid)
    {
        return srid switch
        {
            WellknownSrids.Lambert72 => """PROJCS["Belge_Lambert_1972",GEOGCS["GCS_Belge_1972",DATUM["D_Belge_1972",SPHEROID["International_1924",6378388.0,297.0]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],PROJECTION["Lambert_Conformal_Conic"],PARAMETER["False_Easting",150000.01256],PARAMETER["False_Northing",5400088.4378],PARAMETER["Central_Meridian",4.367486666666666],PARAMETER["Standard_Parallel_1",49.8333339],PARAMETER["Standard_Parallel_2",51.16666723333333],PARAMETER["Latitude_Of_Origin",90.0],UNIT["Meter",1.0]]""",
            WellknownSrids.Lambert08 => """PROJCS["Belge_Lambert_2008",GEOGCS["GCS_ETRS_1989",DATUM["D_ETRS_1989",SPHEROID["GRS_1980",6378137.0,298.257222101]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],PROJECTION["Lambert_Conformal_Conic"],PARAMETER["False_Easting",649328.0],PARAMETER["False_Northing",665262.0],PARAMETER["Central_Meridian",4.359215833333333],PARAMETER["Standard_Parallel_1",49.83333333333334],PARAMETER["Standard_Parallel_2",51.16666666666666],PARAMETER["Latitude_Of_Origin",50.797815],UNIT["Meter",1.0]]""",
            _ => throw new InvalidOperationException(),
        };
    }
}
