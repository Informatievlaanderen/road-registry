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

    // This last one is SQL rather than Linq, and the reason is the index. A segment's street name ids are an
    // array, and Marten translates a Contains() over a duplicated array column to "id = ANY(street_name_ids)"
    // (DuplicatedArrayField.ParseWhereForContains hard-codes it). Postgres cannot answer that from the GIN index on
    // the column: measured on 50.000 segments it is a sequential scan over all of them, where the containment form
    // "street_name_ids @> ARRAY[id]" is a bitmap index scan. Marten has no Linq form that produces containment on a
    // duplicated array - IsSupersetOf leaves the column out of the SQL altogether - so the filter is written by
    // hand. ReadModelQueriesTests in the integration tests covers it against Postgres, which is where that
    // translation can be seen at all.
    //
    // Unqualified column names on purpose: Marten prefixes the select clause it generates with its own alias. The
    // one parameter is wrapped in an explicit object[] because an int[] handed to the params object[] of QueryAsync
    // would otherwise be read as the parameter list itself.
    private const string IsNotRemoved = "(data ->> 'isRemoved' is null or cast(data ->> 'isRemoved' as boolean) = false)";

    // Matches on the segment's street name ids, which exclude the "unknown" and "not applicable" sentinels - a
    // segment without a street name is linked to nothing.
    public static async Task<IReadOnlyList<RoadSegmentReadItem>> FindRoadSegmentsForStreetName(this IQuerySession session, StreetNameLocalId streetNameId, CancellationToken cancellationToken)
    {
        var id = streetNameId.ToInt32();

        var roadSegments = await session.QueryAsync<RoadSegmentReadItem>(
            $"where street_name_ids @> ? and {IsNotRemoved}",
            x => !x.IsRemoved && x.StreetNameIds.Contains(id),
            cancellationToken,
            new object[] { new[] { id } });

        return roadSegments.OrderBy(x => x.Id).ToList();
    }

    // A page of the road segments, in the order of their ids, optionally only the ones linked to a street name and/or
    // in a V2 status. SQL for the same reason as above: the street name filter is the containment match on the GIN
    // index, and the status filter is the status column 20261113120000_add_status_column_to_read_roadsegments generates
    // from the document - the status the detail shows, a V1 road segment's mapped onto V2 - which Marten does not know
    // about. The index on (status, id) answers the status filter and the paging together.
    public static Task<IReadOnlyList<RoadSegmentReadItem>> ListRoadSegments(
        this IQuerySession session,
        StreetNameLocalId? streetNameId,
        RoadSegmentStatusV2? status,
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var conditions = new List<string> { IsNotRemoved };
        var parameters = new List<object>();

        if (streetNameId is not null)
        {
            conditions.Add("street_name_ids @> ?");
            parameters.Add(new[] { streetNameId.Value.ToInt32() });
        }

        if (status is not null)
        {
            conditions.Add("status = ?");
            parameters.Add(status.ToString());
        }

        parameters.Add(offset);
        parameters.Add(limit);

        return session.QueryAsync<RoadSegmentReadItem>(
            $"where {string.Join(" and ", conditions)} order by id offset ? limit ?",
            roadSegments => roadSegments
                .Where(x => !x.IsRemoved)
                .Where(x => streetNameId == null || x.StreetNameIds.Contains(streetNameId.Value.ToInt32()))
                .Where(x => status == null || x.GetStatusV2() == status)
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(limit),
            cancellationToken,
            parameters.ToArray());
    }
}
