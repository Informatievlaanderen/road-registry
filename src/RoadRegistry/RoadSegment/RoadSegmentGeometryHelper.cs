namespace RoadRegistry.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

public static class RoadSegmentGeometryHelper
{
    public static RoadSegmentGeometryDrawMethodV2? DetermineMethod(
        IReadOnlyCollection<(MultiLineString Geometry, RoadSegmentGeometryDrawMethodV2? Method)> segments,
        MultiLineString mergedGeometry)
    {
        if (segments.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segments), "Must have at least one segment");
        }

        if (segments.Any(x => x.Method is null))
        {
            return null;
        }

        var ingemetenTotalLength = segments
            .Where(x => x.Method == RoadSegmentGeometryDrawMethodV2.Ingemeten)
            .Sum(x => x.Geometry.Length);

        var ingemetenPercentage = ingemetenTotalLength / mergedGeometry.Length;
        if (ingemetenPercentage >= RoadSegmentConstants.MinimumPercentageForIngemeten)
        {
            return RoadSegmentGeometryDrawMethodV2.Ingemeten;
        }

        return RoadSegmentGeometryDrawMethodV2.Ingeschetst;
    }
}
