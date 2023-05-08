namespace RoadRegistry.BackOffice.FeatureCompare;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Streams;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using GeometryTranslator = BackOffice.GeometryTranslator;

public class ZipArchiveShapeFileReader
{
    private readonly Encoding _encoding;

    public ZipArchiveShapeFileReader(Encoding encoding)
    {
        _encoding = encoding;
    }

    public IEnumerable<(Geometry, RecordNumber)> Read(ZipArchiveEntry entry)
    {
        //TODO-rik opkuis
        //try
        //{
        return ReadNetTopologySuite(entry);
        //}
        //catch (Exception ex)
        //{
        //    return ReadShaperon(entry);
        //}
    }

    private IEnumerable<(Geometry, RecordNumber)> ReadNetTopologySuite(ZipArchiveEntry entry)
    {
        var recordNumber = RecordNumber.Initial;

        using (var fileStream = entry.Open())
        using (var memoryStream = new MemoryStream())
        {
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            var streamProvider = new ExternallyManagedStreamProvider("Shape", memoryStream);
            var streamProviderRegistry = new ShapefileStreamProviderRegistry(streamProvider, null);
            var shpReader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeReader(streamProviderRegistry);

            foreach (var geometry in shpReader.ReadAllShapes(GeometryConfiguration.GeometryFactory))
            {
                yield return (geometry, recordNumber);
                recordNumber = recordNumber.Next();
            }
        }
    }

    private ICollection<(Geometry, int)> ReadShaperon(ZipArchiveEntry entry)
    {
        var collection = new List<(Geometry, int)>();
        var recordIndex = 0;

        using (var stream = entry.Open())
        using (var reader = new BinaryReader(stream, _encoding))
        {
            var header = ShapeFileHeader.Read(reader);

            using (var records = header.CreateShapeRecordEnumerator(reader))
            {
                while (records.MoveNext())
                {
                    var record = records.Current;
                    if (record?.Content is PointShapeContent pointShapeContent)
                    {
                        collection.Add((GeometryTranslator.ToPoint(pointShapeContent.Shape), recordIndex));
                    }
                    else if (record?.Content is PolyLineMShapeContent polylineShapeContent)
                    {
                        collection.Add((GeometryTranslator.ToMultiLineString(polylineShapeContent.Shape), recordIndex));
                    }
                    else if (record?.Content is PolygonShapeContent polygonShapeContent)
                    {
                        collection.Add((GeometryTranslator.ToMultiPolygon(polygonShapeContent.Shape), recordIndex));
                    }

                    recordIndex++;
                }
            }
        }

        return collection;
    }
}
