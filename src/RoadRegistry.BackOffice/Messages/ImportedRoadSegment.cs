namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ImportedRoadSegment")]
[EventDescription("Indicates a road network segment was imported.")]
public class ImportedRoadSegment : IMessage
{
    public int Id { get; set; }
    public int Version { get; set; }
    public int StartNodeId { get; set; }
    public int EndNodeId { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public int GeometryVersion { get; set; }
    public MaintenanceAuthority MaintenanceAuthority { get; set; }
    public string GeometryDrawMethod { get; set; }
    public string Morphology { get; set; }
    public string Status { get; set; }
    public string Category { get; set; }
    public string AccessRestriction { get; set; }
    public ImportedRoadSegmentSideAttribute LeftSide { get; set; }
    public ImportedRoadSegmentSideAttribute RightSide { get; set; }
    public ImportedRoadSegmentEuropeanRoadAttribute[] PartOfEuropeanRoads { get; set; }
    public ImportedRoadSegmentNationalRoadAttribute[] PartOfNationalRoads { get; set; }
    public ImportedRoadSegmentNumberedRoadAttribute[] PartOfNumberedRoads { get; set; }
    public ImportedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public ImportedRoadSegmentWidthAttribute[] Widths { get; set; }
    public ImportedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public DateTime RecordingDate { get; set; }
    public ImportedOriginProperties Origin { get; set; }
    public string When { get; set; }
}
