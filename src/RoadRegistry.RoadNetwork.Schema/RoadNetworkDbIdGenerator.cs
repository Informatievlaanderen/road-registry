namespace RoadRegistry.RoadNetwork.Schema
{
    using BackOffice;
    using BackOffice.Core;
    using RoadRegistry.ValueObjects;

    public class RoadNetworkDbIdGenerator: IRoadNetworkIdGenerator
    {
        private readonly RoadNetworkDbContext _dbContext;

        public RoadNetworkDbIdGenerator(RoadNetworkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AttributeId NewEuropeanRoadAttributeId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.EuropeanRoadAttributeId);
            return new AttributeId(id);
        }
        public async Task<AttributeId> NewEuropeanRoadAttributeIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.EuropeanRoadAttributeId);
            return new AttributeId(id);
        }

        public GradeSeparatedJunctionId NewGradeSeparatedJunctionId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.GradeSeparatedJunctionId);
            return new GradeSeparatedJunctionId(id);
        }
        public async Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.GradeSeparatedJunctionId);
            return new GradeSeparatedJunctionId(id);
        }

        public AttributeId NewNationalRoadAttributeId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.NationalRoadAttributeId);
            return new AttributeId(id);
        }
        public async Task<AttributeId> NewNationalRoadAttributeIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.NationalRoadAttributeId);
            return new AttributeId(id);
        }

        public AttributeId NewNumberedRoadAttributeId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.NumberedRoadAttributeId);
            return new AttributeId(id);
        }
        public async Task<AttributeId> NewNumberedRoadAttributeIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.NumberedRoadAttributeId);
            return new AttributeId(id);
        }

        public RoadNodeId NewRoadNodeId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.RoadNodeId);
            return new RoadNodeId(id);
        }
        public async Task<RoadNodeId> NewRoadNodeIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadNodeId);
            return new RoadNodeId(id);
        }

        public RoadSegmentId NewRoadSegmentId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.RoadSegmentId);
            return new RoadSegmentId(id);
        }
        public async Task<RoadSegmentId> NewRoadSegmentIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentId);
            return new RoadSegmentId(id);
        }

        public AttributeId NewRoadSegmentLaneAttributeId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.RoadSegmentLaneAttributeId);
            return new AttributeId(id);
        }
        public async Task<AttributeId> NewRoadSegmentLaneAttributeIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentLaneAttributeId);
            return new AttributeId(id);
        }

        public AttributeId NewRoadSegmentSurfaceAttributeId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.RoadSegmentSurfaceAttributeId);
            return new AttributeId(id);
        }
        public async Task<AttributeId> NewRoadSegmentSurfaceAttributeIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentSurfaceAttributeId);
            return new AttributeId(id);
        }

        public AttributeId NewRoadSegmentWidthAttributeId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.RoadSegmentWidthAttributeId);
            return new AttributeId(id);
        }
        public async Task<AttributeId> NewRoadSegmentWidthAttributeIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentWidthAttributeId);
            return new AttributeId(id);
        }

        public TransactionId NewTransactionId()
        {
            var id = _dbContext.GetNextSequenceValue(WellKnownDbSequences.TransactionId);
            return new TransactionId(id);
        }
        public async Task<TransactionId> NewTransactionIdAsync()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.TransactionId);
            return new TransactionId(id);
        }
    }
}
