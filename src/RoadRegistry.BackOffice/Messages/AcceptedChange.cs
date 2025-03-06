namespace RoadRegistry.BackOffice.Messages;

public class AcceptedChange
{
    public Problem[] Problems { get; set; }

    // grade separated junction related
    public GradeSeparatedJunctionAdded GradeSeparatedJunctionAdded { get; set; }
    public GradeSeparatedJunctionModified GradeSeparatedJunctionModified { get; set; }
    public GradeSeparatedJunctionRemoved GradeSeparatedJunctionRemoved { get; set; }

    // node related
    public RoadNodeAdded RoadNodeAdded { get; set; }
    public RoadNodeModified RoadNodeModified { get; set; }
    public RoadNodeRemoved RoadNodeRemoved { get; set; }

    // segment related
    public RoadSegmentAdded RoadSegmentAdded { get; set; }

    // road related
    public RoadSegmentAddedToEuropeanRoad RoadSegmentAddedToEuropeanRoad { get; set; }
    public RoadSegmentAddedToNationalRoad RoadSegmentAddedToNationalRoad { get; set; }
    public RoadSegmentAddedToNumberedRoad RoadSegmentAddedToNumberedRoad { get; set; }
    public RoadSegmentModified RoadSegmentModified { get; set; }
    public RoadSegmentAttributesModified RoadSegmentAttributesModified { get; set; }
    public RoadSegmentGeometryModified RoadSegmentGeometryModified { get; set; }

    public RoadSegmentRemoved RoadSegmentRemoved { get; set; }
    public RoadSegmentsRemoved RoadSegmentsRemoved { get; set; }
    public OutlinedRoadSegmentRemoved OutlinedRoadSegmentRemoved { get; set; }
    public RoadSegmentRemovedFromEuropeanRoad RoadSegmentRemovedFromEuropeanRoad { get; set; }
    public RoadSegmentRemovedFromNationalRoad RoadSegmentRemovedFromNationalRoad { get; set; }
    public RoadSegmentRemovedFromNumberedRoad RoadSegmentRemovedFromNumberedRoad { get; set; }
}
