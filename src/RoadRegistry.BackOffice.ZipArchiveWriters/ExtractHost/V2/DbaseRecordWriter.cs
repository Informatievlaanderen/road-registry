namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using NetTopologySuite.IO.Esri.Shapefiles.Writers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.ZipArchiveWriters.Extensions;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;
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
        IEnumerable<(DbaseRecord, Geometry)> dbaseRecords,
        CancellationToken cancellationToken)
    {
        var dbfFields = dbaseSchema.ToDbfFields();

        var features = dbaseRecords
            .Select(record => record.Item1.ToFeature(record.Item2))
            .ToList();

        await WriteToArchive(archive, fileName, featureType, features, dbfFields, _encoding, cancellationToken);
    }

    private static async Task WriteToArchive(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        ICollection<IFeature> features,
        DbfField[] dbfFields,
        Encoding encoding,
        CancellationToken cancellationToken)
    {
        const string projection = """PROJCS["Belge_Lambert_1972",GEOGCS["GCS_Belge_1972",DATUM["D_Belge_1972",SPHEROID["International_1924",6378388.0,297.0]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],PROJECTION["Lambert_Conformal_Conic"],PARAMETER["False_Easting",150000.01256],PARAMETER["False_Northing",5400088.4378],PARAMETER["Central_Meridian",4.367486666666666],PARAMETER["Standard_Parallel_1",49.8333339],PARAMETER["Standard_Parallel_2",51.16666723333333],PARAMETER["Latitude_Of_Origin",90.0],UNIT["Meter",1.0]]""";

        var shpStream = new MemoryStream();
        var shxStream = new MemoryStream();
        var dbfStream = new MemoryStream();
        var prjStream = new MemoryStream();

        var shapeType = features.FindNonEmptyGeometry().GetShapeType();
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

public static class DbaseExtensions
{
    public static DbfField[] ToDbfFields(this DbaseSchema dbaseSchema)
    {
        return dbaseSchema.Fields
            .Select(field => field.ToDbfField())
            .ToArray();
    }

    public static IFeature ToFeature(this DbaseRecord dbaseRecord, Geometry geometry)
    {
        return new Feature(geometry, dbaseRecord.ToAttributesTable());
    }

    public static AttributesTable ToAttributesTable(this DbaseRecord dbaseRecord)
    {
        var attributes = new AttributesTable();

        foreach (var dbaseFieldValue in dbaseRecord.Values)
        {
            var value = dbaseFieldValue.GetValue();
            attributes.Add(dbaseFieldValue.Field.Name, value);
        }

        return attributes;
    }

    private static DbfField ToDbfField(this DbaseField field)
    {
        switch (field.FieldType)
        {
            case DbaseFieldType.Character:
                return new DbfCharacterField(field.Name, field.Length.ToInt32());
            case DbaseFieldType.Number:
                return new DbfNumericDoubleField(field.Name, field.Length.ToInt32(), field.DecimalCount.ToInt32());
            // case DbaseFieldType.Float:
            //     return new DbfFloatField(field.Name, field.Length.ToInt32(), field.DecimalCount.ToInt32());
            // case DbaseFieldType.Date:
            //     return new DbfDateField(field.Name, field.Length.ToInt32());
            // case DbaseFieldType.Logical:
            //     return new DbfLogicalField(field.Name);
        }

        throw new InvalidOperationException($"Unknown field type: {field.FieldType}");
    }

    private static object GetValue(this DbaseFieldValue dbaseFieldValue)
    {
        if (dbaseFieldValue is DbaseInt32 intField)
        {
            return intField.HasValue ? intField.Value : 0;
        }
        if (dbaseFieldValue is DbaseNullableInt32 nullableIntField)
        {
            return nullableIntField.Value;
        }
        if (dbaseFieldValue is DbaseString stringField)
        {
            return stringField.HasValue ? stringField.Value : null;
        }
        if (dbaseFieldValue is DbaseDateTime dateTimeField)
        {
            return dateTimeField.HasValue ? dateTimeField.Value : null;
        }
        if (dbaseFieldValue is DbaseDouble doubleField)
        {
            return doubleField.HasValue ? doubleField.Value : 0.0;
        }
        if (dbaseFieldValue is DbaseNullableDouble nullableDoubleField)
        {
            return nullableDoubleField.Value;
        }

        throw new InvalidOperationException($"Unknown field type: {dbaseFieldValue.Field.FieldType}");
    }
}

internal static class FeatureExtensions
{
    /// <summary>
    /// Gets default <see cref="ShapeType"/> for specified geometry.
    /// </summary>
    /// <param name="geometry">A Geometry object.</param>
    /// <returns>Shape type.</returns>
    internal static ShapeType GetShapeType(this Geometry geometry)
    {
        geometry = FindNonEmptyGeometry(geometry);

        if (geometry == null || geometry.IsEmpty)
            return ShapeType.NullShape;

        var ordinates = geometry.GetOrdinates();

        if (geometry is Point)
            return GetPointType(ordinates);

        if (geometry is MultiPoint)
            return GetMultiPointType(ordinates);

        if (geometry is LineString || geometry is MultiLineString)
            return GetPolyLineType(ordinates);

        if (geometry is Polygon || geometry is MultiPolygon)
            return GetPolygonType(ordinates);

        throw new ArgumentException("Unsupported shapefile geometry: " + geometry.GetType().Name);
    }

    private static Geometry FindNonEmptyGeometry(Geometry geometry)
    {
        if (geometry == null || geometry.IsEmpty)
            return null;

        var geomColl = geometry as GeometryCollection;

        // Shapefile specification distinguish between Point and MultiPoint.
        // That not the case for PolyLine and Polygon.
        if (geomColl is MultiPoint || geomColl == null)
        {
            return geometry;
        }

        for (int i = 0; i < geomColl.Count; i++)
        {
            var geom = geomColl[i];

            // GeometryCollection -> MultiPolygon -> Polygon
            if (geom is GeometryCollection)
                geom = FindNonEmptyGeometry(geom);

            if (geom != null && !geom.IsEmpty)
                return geom;
        }

        return null;
    }

    internal static Geometry FindNonEmptyGeometry(this IEnumerable<IFeature> features)
    {
        if (features == null)
            return null;

        foreach (var feature in features)
        {
            var geometry = FindNonEmptyGeometry(feature.Geometry);
            if (geometry != null)
                return geometry;
        }

        return null;
    }

    private static Ordinates GetOrdinates(this Geometry geometry)
    {
        if (geometry == null)
            throw new ArgumentNullException(nameof(geometry));

        if (geometry is Point point)
            return point.CoordinateSequence.Ordinates;

        if (geometry is MultiPoint multiPoint)
        {
            return GetOrdinates(multiPoint.Geometries.FirstOrDefault());
        }

        if (geometry is LineString line)
            return line.CoordinateSequence.Ordinates;

        if (geometry is Polygon polygon)
            return polygon.Shell.CoordinateSequence.Ordinates;

        if (geometry.NumGeometries > 0)
            return GetOrdinates(FindNonEmptyGeometry(geometry));

        throw new ArgumentException("Unsupported shapefile geometry: " + geometry.GetType().Name);
    }

    private static ShapeType GetPointType(Ordinates ordinates)
    {
        if (ordinates == Ordinates.XYM)
            return ShapeType.PointM;

        if (ordinates == Ordinates.XYZ || ordinates == Ordinates.XYZM)
            return ShapeType.PointZM;

        return ShapeType.Point;
    }

    private static ShapeType GetMultiPointType(Ordinates ordinates)
    {
        if (ordinates == Ordinates.XYM)
            return ShapeType.MultiPointM;

        if (ordinates == Ordinates.XYZ || ordinates == Ordinates.XYZM)
            return ShapeType.MultiPointZM;

        return ShapeType.MultiPoint;
    }

    private static ShapeType GetPolyLineType(Ordinates ordinates)
    {
        if (ordinates == Ordinates.XYM)
            return ShapeType.PolyLineM;

        if (ordinates == Ordinates.XYZ || ordinates == Ordinates.XYZM)
            return ShapeType.PolyLineZM;

        return ShapeType.PolyLine;
    }

    private static ShapeType GetPolygonType(Ordinates ordinates)
    {
        if (ordinates == Ordinates.XYM)
            return ShapeType.PolygonM;

        if (ordinates == Ordinates.XYZ || ordinates == Ordinates.XYZM)
            return ShapeType.PolygonZM;

        return ShapeType.Polygon;
    }
}
