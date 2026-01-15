namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using ScopedRoadNetwork;
using ValueObjects.Problems;

public partial class RoadNode
{
    public static (RoadNode?, Problems) Add(AddRoadNodeChange change, Provenance provenance, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.For(change.TemporaryId);

        var roadNode = Create(new RoadNodeWasAdded
        {
            RoadNodeId = idGenerator.NewRoadNodeId(),
            OriginalId = change.OriginalId ?? change.TemporaryId,
            Geometry = change.Geometry,
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return (roadNode, problems);
    }
}
