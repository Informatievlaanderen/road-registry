namespace RoadRegistry.BackOffice.ShapeFile.V2;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase.V2;
using Extracts;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using NetTopologySuite.IO.Esri.Shapefiles.Writers;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class ShapeFileRecordWriter
{
    private readonly Encoding _encoding;

    public ShapeFileRecordWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
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
        const string projection = """PROJCS["Belge_Lambert_1972",GEOGCS["GCS_Belge_1972",DATUM["D_Belge_1972",SPHEROID["International_1924",6378388.0,297.0]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],PROJECTION["Lambert_Conformal_Conic"],PARAMETER["False_Easting",150000.01256],PARAMETER["False_Northing",5400088.4378],PARAMETER["Central_Meridian",4.367486666666666],PARAMETER["Standard_Parallel_1",49.8333339],PARAMETER["Standard_Parallel_2",51.16666723333333],PARAMETER["Latitude_Of_Origin",90.0],UNIT["Meter",1.0]]""";

        var shpStream = new MemoryStream();
        var shxStream = new MemoryStream();
        var dbfStream = new MemoryStream();
        var prjStream = new MemoryStream();

        var options = new ShapefileWriterOptions(shapeType, dbfFields)
        {
            Projection = projection,
            Encoding = _encoding
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

    private static async Task CopyToEntry(MemoryStream stream, ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        await using var entryStream = entry.Open();

        stream.Position = 0;
        await stream.CopyToAsync(entryStream, cancellationToken);

        await entryStream.FlushAsync(cancellationToken);
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
}
