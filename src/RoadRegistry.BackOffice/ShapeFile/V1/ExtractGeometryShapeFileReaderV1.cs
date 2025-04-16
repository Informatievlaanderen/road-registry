namespace RoadRegistry.BackOffice.ShapeFile.V1;

using System.IO;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;

public class ExtractGeometryShapeFileReaderV1
{
    public (ShapefileHeader, Geometry) Read(Stream stream, GeometryFactory geometryFactory = null)
    {
        stream.Position = 0;

        var streamProvider = new ExternallyManagedStreamProvider(StreamTypes.Shape, stream);
        var streamProviderRegistry = new ShapefileStreamProviderRegistry(streamProvider, null);

        var shpReader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeReader(streamProviderRegistry);
        var geometry = shpReader.ReadAllShapes(geometryFactory ?? WellKnownGeometryFactories.WithoutMAndZ).FirstOrDefault();

        return (shpReader.ShapefileHeader, geometry);
    }
}
