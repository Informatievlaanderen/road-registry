namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Union;

public sealed class OgcFeaturesCache
{
    private readonly STRtree<Geometry> _polygonIndex;

    public OgcFeaturesCache(IReadOnlyList<OgcFeature> features)
    {
        _polygonIndex = new STRtree<Geometry>();
        foreach (var feature in features)
        {
            _polygonIndex.Insert(feature.Geometry.EnvelopeInternal, feature.Geometry);
        }
    }

    public bool HasOverlapWithFeatures(MultiLineString geometry, double minimumOverlapPercentage)
    {
        var lineString = geometry;
        var totalLength = lineString.Length;

        // Query spatial index for potentially intersecting polygons
        var candidates = _polygonIndex.Query(lineString.EnvelopeInternal);
        if (candidates.Count == 0)
        {
            return false;
        }

        // Union all intersecting polygons for this segment
        var combinedPolygon = UnionPolygons(candidates);

        // Calculate intersection length
        var intersection = lineString.Intersection(combinedPolygon);
        var coveredLength = intersection.Length;

        var coveragePercentage = coveredLength / totalLength;

        if (coveragePercentage >= minimumOverlapPercentage)
        {
            return true;
        }

        return false;
    }

    private static Geometry UnionPolygons(IList<Geometry> polygons)
    {
        if (polygons.Count == 1)
        {
            return polygons[0];
        }

        return CascadedPolygonUnion.Union(polygons);
    }
}
