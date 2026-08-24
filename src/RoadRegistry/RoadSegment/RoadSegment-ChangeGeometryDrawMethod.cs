namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public void ChangeGeometryDrawMethod(RoadSegmentGeometryDrawMethodV2 geometryDrawMethod, ScopedRoadNetworkChangeContext context)
    {
        // Setting the draw method to the value it already has is accepted, but records nothing: an event saying
        // nothing changed has no place in the stream.
        if (Attributes!.GeometryDrawMethod == geometryDrawMethod)
        {
            return;
        }

        Apply(new RoadSegmentGeometryDrawMethodWasChanged
        {
            RoadSegmentId = RoadSegmentId,
            GeometryDrawMethod = geometryDrawMethod,
            Provenance = new ProvenanceData(context.Provenance)
        });
    }

    public void Apply(RoadSegmentGeometryDrawMethodWasChanged @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            GeometryDrawMethod = @event.GeometryDrawMethod
        };
    }
}
