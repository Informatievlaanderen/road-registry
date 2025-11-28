namespace RoadRegistry.BackOffice.Api.RoadSegments
{
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;

    public interface IRoadSegmentRepository
    {
        Task<RoadSegmentRecord> FindAsync(RoadSegmentId id, CancellationToken cancellationToken);
    }

    public class RoadSegmentRepository : IRoadSegmentRepository
    {
        private readonly IRoadRegistryContext _roadRegistryContext;
        private readonly EditorContext _editorContext;

        public RoadSegmentRepository(
            IRoadRegistryContext roadRegistryContext,
            EditorContext editorContext)
        {
            _roadRegistryContext = roadRegistryContext;
            _editorContext = editorContext;
        }

        public async Task<RoadSegmentRecord> FindAsync(RoadSegmentId id, CancellationToken cancellationToken)
        {
            {
                var roadNetwork = await _roadRegistryContext.RoadNetworks.ForOutlinedRoadSegment(id, cancellationToken);
                var roadSegment = roadNetwork.FindRoadSegment(id);
                if (roadSegment is not null && roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
                {
                    return new RoadSegmentRecord(id, roadSegment.AttributeHash.GeometryDrawMethod, roadSegment.LastEventHash);
                }
            }

            {
                var roadSegment = await _editorContext.RoadSegments.FindAsync([id.ToInt32()], cancellationToken);
                if (roadSegment is not null)
                {
                    return new RoadSegmentRecord(id, RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegment.MethodId], roadSegment.LastEventHash);
                }
            }

            return null;
        }
    }

    public sealed record RoadSegmentRecord(RoadSegmentId Id, RoadSegmentGeometryDrawMethod GeometryDrawMethod, string LastEventHash);
}
