namespace RoadRegistry.Extracts.Infrastructure.ShapeFile;

using System.IO;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using NetTopologySuite.IO.Esri.Shp;

public class ExtractGeometryShapeFileReaderV2
{
    public (ShapeType, Geometry?) Read(Stream stream, GeometryFactory geometryFactory)
    {
        stream.Position = 0;

        var shp = Shp.OpenRead(stream, new ShapefileReaderOptions
        {
            Factory = geometryFactory
        });

        var geometry = shp.FirstOrDefault();

        return (shp.ShapeType, geometry);
    }
}
