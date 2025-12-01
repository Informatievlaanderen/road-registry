namespace RoadRegistry.RoadNetwork;

using System.Threading.Tasks;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

public interface IRoadNetworkIdGenerator
{
    TransactionId NewTransactionId();
    Task<TransactionId> NewTransactionIdAsync();
    RoadNodeId NewRoadNodeId();
    Task<RoadNodeId> NewRoadNodeIdAsync();
    RoadSegmentId NewRoadSegmentId();
    Task<RoadSegmentId> NewRoadSegmentIdAsync();
    AttributeId NewRoadSegmentLaneAttributeId();
    Task<AttributeId> NewRoadSegmentLaneAttributeIdAsync();
    AttributeId NewRoadSegmentSurfaceAttributeId();
    Task<AttributeId> NewRoadSegmentSurfaceAttributeIdAsync();
    AttributeId NewRoadSegmentWidthAttributeId();
    Task<AttributeId> NewRoadSegmentWidthAttributeIdAsync();
    AttributeId NewEuropeanRoadAttributeId();
    Task<AttributeId> NewEuropeanRoadAttributeIdAsync();
    AttributeId NewNationalRoadAttributeId();
    Task<AttributeId> NewNationalRoadAttributeIdAsync();
    AttributeId NewNumberedRoadAttributeId();
    Task<AttributeId> NewNumberedRoadAttributeIdAsync();
    GradeSeparatedJunctionId NewGradeSeparatedJunctionId();
    Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionIdAsync();
}
