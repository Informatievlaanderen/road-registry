namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using Microsoft.EntityFrameworkCore;
using Product.Schema;

// What the product no longer contains because it is ingewonnen: every road segment whose inwinningsstatus is
// 'compleet', every road node one of those road segments starts or ends at, and - through those road segments - their
// attributes and every grade separated junction at least one of them crosses at.
public sealed class ProductInwinningFilter
{
    public static readonly ProductInwinningFilter None = new([], []);

    private readonly HashSet<int> _roadSegmentIds;
    private readonly HashSet<int> _roadNodeIds;

    public ProductInwinningFilter(IEnumerable<int> roadSegmentIds, IEnumerable<int> roadNodeIds)
    {
        _roadSegmentIds = roadSegmentIds.ToHashSet();
        _roadNodeIds = roadNodeIds.ToHashSet();
    }

    // The road nodes follow from the product's own road segments, as they are in the snapshot the archive is built from.
    public static async Task<ProductInwinningFilter> CreateAsync(IEnumerable<int> completedRoadSegmentIds, ProductContext context, CancellationToken cancellationToken)
    {
        var roadSegmentIds = completedRoadSegmentIds.ToHashSet();
        if (roadSegmentIds.Count == 0)
        {
            return None;
        }

        var roadNodeIds = new HashSet<int>();
        await foreach (var roadSegment in context.RoadSegments
                           .AsNoTracking()
                           .Select(x => new { x.Id, x.StartNodeId, x.EndNodeId })
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            if (roadSegmentIds.Contains(roadSegment.Id))
            {
                roadNodeIds.Add(roadSegment.StartNodeId);
                roadNodeIds.Add(roadSegment.EndNodeId);
            }
        }

        return new ProductInwinningFilter(roadSegmentIds, roadNodeIds);
    }

    // Counts what remains of the records these road segment ids belong to, one id per record.
    public async Task<int> CountRoadSegmentRecordsAsync(IQueryable<int> roadSegmentIds, CancellationToken cancellationToken)
    {
        if (_roadSegmentIds.Count == 0)
        {
            return await roadSegmentIds.CountAsync(cancellationToken);
        }

        var count = 0;
        await foreach (var roadSegmentId in roadSegmentIds.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            if (!ExcludesRoadSegment(roadSegmentId))
            {
                count++;
            }
        }

        return count;
    }

    public bool ExcludesAnything => _roadSegmentIds.Count > 0 || _roadNodeIds.Count > 0;

    public bool ExcludesRoadSegment(int roadSegmentId) => _roadSegmentIds.Contains(roadSegmentId);

    public bool ExcludesRoadNode(int roadNodeId) => _roadNodeIds.Contains(roadNodeId);

    public bool ExcludesGradeSeparatedJunction(int upperRoadSegmentId, int lowerRoadSegmentId) =>
        ExcludesRoadSegment(upperRoadSegmentId) || ExcludesRoadSegment(lowerRoadSegmentId);
}
