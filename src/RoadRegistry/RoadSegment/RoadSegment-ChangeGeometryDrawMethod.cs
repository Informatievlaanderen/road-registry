namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public void ChangeGeometryDrawMethod(RoadSegmentGeometryDrawMethodV2 geometryDrawMethod, ScopedRoadNetworkChangeContext context)
    {
        ChangeGeometryDrawMethod(geometryDrawMethod, context.Provenance);
    }

    // Setting the draw method to the value it already has is accepted, but records nothing: an event saying nothing
    // changed has no place in the stream. The draw method it takes on is the event itself.
    private void ChangeGeometryDrawMethod(RoadSegmentGeometryDrawMethodV2 geometryDrawMethod, Provenance provenance)
    {
        if (Attributes!.GeometryDrawMethod == geometryDrawMethod)
        {
            return;
        }

        if (geometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten)
        {
            Apply(new RoadSegmentWasMeasured
            {
                RoadSegmentId = RoadSegmentId,
                Provenance = new ProvenanceData(provenance)
            });
        }
        else
        {
            Apply(new RoadSegmentWasCorrectedToOutlined
            {
                RoadSegmentId = RoadSegmentId,
                Provenance = new ProvenanceData(provenance)
            });
        }
    }

    public void Apply(RoadSegmentWasMeasured @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten
        };
    }

    public void Apply(RoadSegmentWasCorrectedToOutlined @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
        };
    }
}
