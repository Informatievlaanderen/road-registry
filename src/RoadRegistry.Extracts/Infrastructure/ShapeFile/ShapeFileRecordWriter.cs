namespace RoadRegistry.Extracts.Infrastructure.ShapeFile;

using System.Buffers.Binary;
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
    private const int ShapeFileHeaderLengthInBytes = 100;
    private const int ShapeFileHeaderFileLengthOffset = 24;

    private readonly Encoding _encoding;
    private readonly string _projection;

    public ShapeFileRecordWriter(Encoding encoding)
        : this(encoding, ProjectionFormat.BelgeLambert1972)
    {
    }

    public ShapeFileRecordWriter(Encoding encoding, ProjectionFormat projectionFormat)
    {
        _encoding = encoding.ThrowIfNull();
        _projection = projectionFormat.Content;
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

        if (features.Count == 0)
        {
            WriteShapeFileLengthHeader(shpStream);
            WriteShapeFileLengthHeader(shxStream);
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

    /// <summary>
    ///     Writes the actual file length, expressed in 16-bit words, into the header of a .shp/.shx file.
    /// </summary>
    /// <remarks>
    ///     When no features are written, NetTopologySuite.IO.Esri leaves the file length in the .shp/.shx header at 0
    ///     instead of the 50 words the header itself occupies. GDAL based software (QGIS, ArcGIS, ...) derives the number
    ///     of shapes from the .shx file length ((0 * 2 - 100) / 8 = -12) and refuses to open the shapefile with
    ///     "Number of shapes does not match the number of table records".
    /// </remarks>
    private static void WriteShapeFileLengthHeader(MemoryStream stream)
    {
        if (stream.Length < ShapeFileHeaderLengthInBytes)
        {
            return;
        }

        Span<byte> fileLengthInWords = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(fileLengthInWords, (int)(stream.Length / 2));

        stream.Position = ShapeFileHeaderFileLengthOffset;
        stream.Write(fileLengthInWords);
        stream.Position = 0;
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
}
