namespace RoadRegistry.Read.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ValueObjects;

// The read documents do not point at each other: each one carries only the foreign keys it owns, duplicated into
// indexed columns of its own table (see each projection's Configure). Whatever needs the other direction - the
// segments of a road node, the crossings of a segment, the segments of a street name - asks for it here, when it
// needs it, instead of a projection keeping a list up to date on the other document.
//
// That is what frees the read projection from having to see a correlation's events in emission order: an event now
// only ever touches the rows of its own entity, so a segment can be projected before the node it points at exists.
public static class ReadModelQueries
{
    // Every segment knotted into this road node. A segment that is not realized carries no nodes at all, and a
    // removed segment keeps the nodes it had, so both sides are filtered out here.
    public static Task<IReadOnlyList<RoadSegmentReadItem>> FindRoadSegmentsForRoadNode(this IQuerySession session, RoadNodeId roadNodeId, CancellationToken cancellationToken)
    {
        var nodeId = roadNodeId.ToInt32();

        return session.Query<RoadSegmentReadItem>()
            .Where(x => !x.IsRemoved && (x.StartNodeId == nodeId || x.EndNodeId == nodeId))
            .OrderBy(x => x.Id)
            .ToReadOnlyListAsync(cancellationToken);
    }

    public static Task<IReadOnlyList<GradeJunctionReadItem>> FindGradeJunctionsForRoadSegment(this IQuerySession session, RoadSegmentId roadSegmentId, CancellationToken cancellationToken)
    {
        var segmentId = roadSegmentId.ToInt32();

        return session.Query<GradeJunctionReadItem>()
            .Where(x => !x.IsRemoved && (x.RoadSegmentId1 == segmentId || x.RoadSegmentId2 == segmentId))
            .OrderBy(x => x.Id)
            .ToReadOnlyListAsync(cancellationToken);
    }

    public static Task<IReadOnlyList<GradeSeparatedJunctionReadItem>> FindGradeSeparatedJunctionsForRoadSegment(this IQuerySession session, RoadSegmentId roadSegmentId, CancellationToken cancellationToken)
    {
        var segmentId = roadSegmentId.ToInt32();

        return session.Query<GradeSeparatedJunctionReadItem>()
            .Where(x => !x.IsRemoved && (x.LowerRoadSegmentId == segmentId || x.UpperRoadSegmentId == segmentId))
            .OrderBy(x => x.Id)
            .ToReadOnlyListAsync(cancellationToken);
    }

    // Matches on the segment's street name ids, which exclude the "unknown" and "not applicable" sentinels - a
    // segment without a street name is linked to nothing.
    public static Task<IReadOnlyList<RoadSegmentReadItem>> FindRoadSegmentsForStreetName(this IQuerySession session, StreetNameLocalId streetNameId, CancellationToken cancellationToken)
    {
        var id = streetNameId.ToInt32();

        return session.Query<RoadSegmentReadItem>()
            .Where(x => !x.IsRemoved && x.StreetNameIds.Contains(id))
            .OrderBy(x => x.Id)
            .ToReadOnlyListAsync(cancellationToken);
    }

    public static Task<IReadOnlyList<RoadSegmentReadItem>> FindRoadSegmentsForOrganization(this IQuerySession session, OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var id = organizationId.ToString();

        return session.Query<RoadSegmentReadItem>()
            .Where(x => !x.IsRemoved && x.MaintenanceAuthorityIds.Contains(id))
            .OrderBy(x => x.Id)
            .ToReadOnlyListAsync(cancellationToken);
    }
}
