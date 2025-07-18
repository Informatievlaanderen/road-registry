namespace RoadRegistry.BackOffice.Messages;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

[EventName(EventName)]
[EventDescription("Indicates a road network segment was imported.")]
public class ImportedRoadSegment : IMessage, IHaveHash, IWhen
{
    public const string EventName = "ImportedRoadSegment";

    public string AccessRestriction { get; set; }
    public string Category { get; set; }
    public int EndNodeId { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public string GeometryDrawMethod { get; set; }
    public int GeometryVersion { get; set; }
    public int Id { get; set; }
    public ImportedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public ImportedRoadSegmentSideAttribute LeftSide { get; set; }
    public MaintenanceAuthority MaintenanceAuthority { get; set; }
    public string Morphology { get; set; }
    public ImportedOriginProperties Origin { get; set; }
    public ImportedRoadSegmentEuropeanRoadAttribute[] PartOfEuropeanRoads { get; set; }
    public ImportedRoadSegmentNationalRoadAttribute[] PartOfNationalRoads { get; set; }
    public ImportedRoadSegmentNumberedRoadAttribute[] PartOfNumberedRoads { get; set; }
    public DateTime RecordingDate { get; set; }
    public ImportedRoadSegmentSideAttribute RightSide { get; set; }
    public int StartNodeId { get; set; }
    public string Status { get; set; }
    public ImportedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public int Version { get; set; }
    public string When { get; set; }
    public ImportedRoadSegmentWidthAttribute[] Widths { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
