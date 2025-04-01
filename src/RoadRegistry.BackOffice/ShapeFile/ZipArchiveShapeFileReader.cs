namespace RoadRegistry.BackOffice.ShapeFile;

using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using NetTopologySuite.IO.Esri.Shp;

public class ZipArchiveShapeFileReader
{
    public IEnumerable<(Geometry, RecordNumber)> Read(ZipArchiveEntry entry)
    {
        var recordNumber = RecordNumber.Initial;

        using var fileStream = entry.Open();
        using var memoryStream = new MemoryStream();

        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var geometries = Shp.OpenRead(memoryStream, new ShapefileReaderOptions
        {
            Factory = WellKnownGeometryFactories.WithoutMAndZ
        }).ToArray();

        foreach (var geometry in geometries)
        {
            yield return (geometry, recordNumber);
            recordNumber = recordNumber.Next();
        }
    }
}
