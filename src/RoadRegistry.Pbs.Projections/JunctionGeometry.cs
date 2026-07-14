namespace RoadRegistry.Pbs.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using Schema;

// A kruising (junction) carries no geometry of its own on the events: its point is the first intersection coordinate of
// the two road segments it links. The linked segments are always added before the junction that references them, and
// events are processed one at a time in sequence order, so by the time a junction event is handled the segment geometry
// is already stored in the RoadSegments table. Returns null when either segment (or the intersection) is not available.
internal static class JunctionGeometry
{
    public static async Task<Geometry> FirstIntersection(PbsContext context, int roadSegmentId1, int roadSegmentId2, CancellationToken ct)
    {
        var geometry1 = await context.RoadSegments.AsNoTracking().Where(x => x.WS_OIDN == roadSegmentId1).Select(x => x.GEOMETRIE).FirstOrDefaultAsync(ct);
        var geometry2 = await context.RoadSegments.AsNoTracking().Where(x => x.WS_OIDN == roadSegmentId2).Select(x => x.GEOMETRIE).FirstOrDefaultAsync(ct);
        if (geometry1 is null || geometry2 is null)
        {
            return null;
        }

        Geometry intersection;
        try
        {
            intersection = geometry1.Intersection(geometry2);
        }
        catch (TopologyException)
        {
            return null;
        }

        if (intersection.IsEmpty)
        {
            return null;
        }

        return geometry1.Factory.CreatePoint(intersection.Coordinate.RoundToCm());
    }
}
