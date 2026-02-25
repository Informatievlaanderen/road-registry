namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;

public sealed class OgcFeaturesCache
{
    private readonly IReadOnlyList<OgcFeature> _features;

    public OgcFeaturesCache(IReadOnlyList<OgcFeature> features)
    {
        _features = features;
    }

    public bool HasOverlapWithFeatures(MultiLineString geometry, double minimumOverlapPercentage)
    {
        var source = geometry.Buffer(0.001);
        var overlappingFeatures = _features
            .Select(x => (Feature: x, OverlapPercentage: GetOverlapPercentage(source, x)))
            .Where(x => x.OverlapPercentage > 0)
            .ToList();

        return overlappingFeatures.Sum(x => x.OverlapPercentage) >= minimumOverlapPercentage;
    }

    private static double GetOverlapPercentage(Geometry roadSegmentGeometry, OgcFeature ogcFeature)
    {
        var overlap = roadSegmentGeometry.Intersection(ogcFeature.Geometry);

        var overlapValue = overlap.Area / roadSegmentGeometry.Area;
        return overlapValue;
    }
}
