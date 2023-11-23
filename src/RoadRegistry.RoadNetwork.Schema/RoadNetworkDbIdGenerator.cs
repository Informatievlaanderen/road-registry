namespace RoadRegistry.RoadNetwork.Schema
{
    using BackOffice;
    using BackOffice.Core;

    public class RoadNetworkDbIdGenerator: IRoadNetworkIdGenerator
    {
        private readonly RoadNetworkDbContext _dbContext;

        public RoadNetworkDbIdGenerator(RoadNetworkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AttributeId> NewEuropeanRoadAttributeId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.EuropeanRoadAttributeId);
            return new AttributeId(id);
        }

        public async Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.GradeSeparatedJunctionId);
            return new GradeSeparatedJunctionId(id);
        }

        public async Task<AttributeId> NewNationalRoadAttributeId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.NationalRoadAttributeId);
            return new AttributeId(id);
        }

        public async Task<AttributeId> NewNumberedRoadAttributeId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.NumberedRoadAttributeId);
            return new AttributeId(id);
        }

        public async Task<RoadNodeId> NewRoadNodeId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadNodeId);
            return new RoadNodeId(id);
        }

        public async Task<RoadSegmentId> NewRoadSegmentId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentId);
            return new RoadSegmentId(id);
        }

        public async Task<AttributeId> NewRoadSegmentLaneAttributeId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentLaneAttributeId);
            return new AttributeId(id);
        }

        public async Task<AttributeId> NewRoadSegmentSurfaceAttributeId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentSurfaceAttributeId);
            return new AttributeId(id);
        }

        public async Task<AttributeId> NewRoadSegmentWidthAttributeId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.RoadSegmentWidthAttributeId);
            return new AttributeId(id);
        }

        public async Task<TransactionId> NewTransactionId()
        {
            var id = await _dbContext.GetNextSequenceValueAsync(WellKnownDbSequences.TransactionId);
            return new TransactionId(id);
        }
    }
}
