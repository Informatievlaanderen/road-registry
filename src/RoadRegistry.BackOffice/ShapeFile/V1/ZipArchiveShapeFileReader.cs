namespace RoadRegistry.BackOffice.ShapeFile.V1;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.ShapeFile.Extended;
using NetTopologySuite.IO.Streams;

[Obsolete("Use V2 ShapeFileRecordReader instead")]
public class ZipArchiveShapeFileReader
{
    public IEnumerable<(Geometry, RecordNumber)> Read(ZipArchiveEntry entry)
    {
        var recordNumber = RecordNumber.Initial;

        using var fileStream = entry.Open();
        using var memoryStream = new MemoryStream();

        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var streamProvider = new ExternallyManagedStreamProvider(StreamTypes.Shape, memoryStream);
        var streamProviderRegistry = new ShapefileStreamProviderRegistry(streamProvider, null);
        var shpReader = new ShapeReader(streamProviderRegistry);

        foreach (var geometry in shpReader.ReadAllShapes(WellKnownGeometryFactories.Lambert72))
        {
            yield return (geometry, recordNumber);
            recordNumber = recordNumber.Next();
        }
    }
}
