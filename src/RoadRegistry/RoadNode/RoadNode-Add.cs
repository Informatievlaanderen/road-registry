namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using RoadNetwork;
using ValueObjects.Problems;

public partial class RoadNode
{
    public static (RoadNode?, Problems) Add(AddRoadNodeChange change, Provenance provenance, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;

        var roadNode = Create(new RoadNodeAdded
        {
            RoadNodeId = idGenerator.NewRoadNodeId(),
            OriginalId = change.OriginalId ?? change.TemporaryId,
            Geometry = change.Geometry.ToGeometryObject(),
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return (roadNode, problems);
    }
}
