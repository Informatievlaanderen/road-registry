namespace RoadRegistry.BackOffice.Core;

using System.Threading.Tasks;

//TODO-rik hier een DBcontext variant van maken, wie moet deze kennen? commandhost al zeker en commandhost moet de dbmigrations uitvoeren
//de lambdas ook
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
