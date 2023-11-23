namespace RoadRegistry.BackOffice.Core;

using System.Threading.Tasks;

public interface IRoadNetworkIdGenerator
{
    Task<AttributeId> NewEuropeanRoadAttributeId();
    Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionId();
    Task<AttributeId> NewNationalRoadAttributeId();
    Task<AttributeId> NewNumberedRoadAttributeId();
    Task<RoadNodeId> NewRoadNodeId();
    Task<RoadSegmentId> NewRoadSegmentId();
    Task<AttributeId> NewRoadSegmentLaneAttributeId();
    Task<AttributeId> NewRoadSegmentSurfaceAttributeId();
    Task<AttributeId> NewRoadSegmentWidthAttributeId();
    Task<TransactionId> NewTransactionId();
}
