namespace RoadRegistry.Tests;

using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

public class FakeGrbOgcApiFeaturesDownloader : IGrbOgcApiFeaturesDownloader
{
    private readonly List<Geometry> _geometries;

    public FakeGrbOgcApiFeaturesDownloader()
        : this([])
    {
    }

    public FakeGrbOgcApiFeaturesDownloader(IEnumerable<Geometry> geometries)
    {
        _geometries = geometries.ToList();
    }

    public void Add(Geometry geometry) => AddRange([geometry]);

    public void AddRange(IEnumerable<Geometry> geometries)
    {
        _geometries.AddRange(geometries.Select(x => x is Polygon or MultiPolygon ? x : x.Buffer(0.01)));
    }

    public Task<IReadOnlyList<OgcFeature>> DownloadFeaturesAsync(IEnumerable<string> collectionIds, Envelope boundingBox, int srid, CancellationToken cancellationToken)
    {
        var features = collectionIds
            .SelectMany(collectionId => _geometries.Select(x => new OgcFeature(collectionId, null, x, null)))
            .ToList();
        return Task.FromResult<IReadOnlyList<OgcFeature>>(features);
    }
}
