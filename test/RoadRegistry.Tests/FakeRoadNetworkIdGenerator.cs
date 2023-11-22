namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Core;

    public class FakeRoadNetworkIdGenerator: IRoadNetworkIdGenerator
    {
        public Task<AttributeId> NewEuropeanRoadAttributeId()
        {
            return Task.FromResult(new AttributeId(1));
        }

        public Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionId()
        {
            return Task.FromResult(new GradeSeparatedJunctionId(1));
        }

        public Task<AttributeId> NewNationalRoadAttributeId()
        {
            return Task.FromResult(new AttributeId(1));
        }

        public Task<AttributeId> NewNumberedRoadAttributeId()
        {
            return Task.FromResult(new AttributeId(1));
        }

        public Task<RoadNodeId> NewRoadNodeId()
        {
            return Task.FromResult(new RoadNodeId(1));
        }

        public Task<RoadSegmentId> NewRoadSegmentId()
        {
            return Task.FromResult(new RoadSegmentId(1));
        }

        public Task<AttributeId> NewRoadSegmentLaneAttributeId()
        {
            return Task.FromResult(new AttributeId(1));
        }

        public Task<AttributeId> NewRoadSegmentSurfaceAttributeId()
        {
            return Task.FromResult(new AttributeId(1));
        }

        public Task<AttributeId> NewRoadSegmentWidthAttributeId()
        {
            return Task.FromResult(new AttributeId(1));
        }

        public Task<TransactionId> NewTransactionId()
        {
            return Task.FromResult(new TransactionId(1));
        }
    }
}
