namespace RoadRegistry.BackOffice.Api.RoadSegments
{
    using BackOffice.Extracts.Dbase.RoadSegments;
    using Editor.Schema;
    using Editor.Schema.Extensions;
    using Microsoft.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRoadSegmentRepository
    {
        Task<RoadSegmentRecord> Find(RoadSegmentId id, CancellationToken cancellationToken);
    }

    public class RoadSegmentRepository: IRoadSegmentRepository
    {
        private readonly IRoadRegistryContext _roadRegistryContext;
        private readonly EditorContext _editorContext;
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly FileEncoding _fileEncoding;

        public RoadSegmentRepository(
            IRoadRegistryContext roadRegistryContext,
            EditorContext editorContext,
            RecyclableMemoryStreamManager manager,
            FileEncoding fileEncoding)
        {
            _roadRegistryContext = roadRegistryContext;
            _editorContext = editorContext;
            _manager = manager;
            _fileEncoding = fileEncoding;
        }

        public async Task<RoadSegmentRecord> Find(RoadSegmentId id, CancellationToken cancellationToken)
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
                var roadSegment = await _editorContext.RoadSegments.FindAsync(new object[] { id.ToInt32() }, cancellationToken);
                if (roadSegment is not null)
                {
                    var roadSegmentDbaseRecord = new RoadSegmentDbaseRecord().FromBytes(roadSegment.DbaseRecord, _manager, _fileEncoding);
                    return new RoadSegmentRecord(id, RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord.METHODE.Value], roadSegment.LastEventHash);
                }
            }

            return null;
        }
    }

    public sealed record RoadSegmentRecord(RoadSegmentId Id, RoadSegmentGeometryDrawMethod GeometryDrawMethod, string LastEventHash);
}
