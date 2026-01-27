namespace RoadRegistry.Extracts.ZipArchiveWriters;

using System.Reflection;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;
using RoadRegistry.Infrastructure;

public static class GewestGrens
{
    private static readonly IReadOnlyCollection<LineString> BorderGeometries;

    static GewestGrens()
    {
        using var shpStream = Read("Resources.Refgew.shp");
        using var shxStream = Read("Resources.Refgew.shx");

        var shpStreamProvider = new ExternallyManagedStreamProvider(StreamTypes.Shape, shpStream);
        var shxStreamProvider = new ExternallyManagedStreamProvider(StreamTypes.Index, shxStream);
        var streamProviderRegistry = new ShapefileStreamProviderRegistry(shpStreamProvider, null, shxStreamProvider);

        var shpFileReader = new ShapefileReader(streamProviderRegistry, WellKnownGeometryFactories.Lambert72);

        BorderGeometries = ((MultiPolygon)shpFileReader.ReadAll()[0]).Geometries.OfType<Polygon>().Select(x => x.EnsureLambert08().ExteriorRing).ToArray();
    }

    public static bool IsCloseToBorder(Point point, double distance)
    {
        if (!point.IsLambert08())
        {
            throw new ArgumentException("Point is not Lambert08");
        }

        return BorderGeometries.Any(border => point.Distance(border) < distance);
    }

    private static MemoryStream Read(string fileName)
    {
        if (!TryFindEmbeddedResourceName(fileName, out var resourceAssembly, out var resourceName))
        {
            throw new FileNotFoundException("Embedded resource not found!", fileName);
        }

        var sourceStream = new MemoryStream();
        var embeddedStream = resourceAssembly!.GetManifestResourceStream(resourceName!);
        embeddedStream!.CopyTo(sourceStream);
        sourceStream.Position = 0;

        return sourceStream;
    }

    private static bool TryFindEmbeddedResourceName(string fileName, out Assembly? resourceAssembly, out string? resourceName)
    {
        resourceAssembly = Assembly.GetExecutingAssembly();
        var resourceNames = resourceAssembly.GetManifestResourceNames();

        resourceName = resourceNames
            .SingleOrDefault(embeddedResource => embeddedResource.EndsWith($".{fileName}", StringComparison.InvariantCultureIgnoreCase));

        if (resourceName is not null)
        {
            return true;
        }

        resourceAssembly = null;
        resourceName = null;
        return false;
    }
}
