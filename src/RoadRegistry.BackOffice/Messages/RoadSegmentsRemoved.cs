namespace RoadRegistry.BackOffice.Messages;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentsRemoved : IHaveHash
{
    public const string EventName = "RoadSegmentsRemoved";

    public string GeometryDrawMethod { get; set; }
    public int[] RemovedRoadSegmentIds { get; set; } = [];
    public RoadSegmentMerged[] MergedRoadSegments { get; set; } = [];
    public int[] RemovedRoadNodeIds { get; set; } = [];
    public RoadNodeTypeChanged[] ChangedRoadNodes { get; set; } = [];
    public int[] RemovedGradeSeparatedJunctionIds { get; set; } = [];

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}

public class RoadNodeTypeChanged : IHaveHashFields
{
    public int Id { get; set; }
    public int Version { get; set; }
    public string Type { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
}

public class RoadSegmentMerged : IHaveHashFields
{
    public int Id { get; set; }
    public int Version { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public int GeometryVersion { get; set; }
    public int StartNodeId { get; set; }
    public int EndNodeId { get; set; }
    public string GeometryDrawMethod { get; set; }
    public string AccessRestriction { get; set; }
    public string Category { get; set; }
    public MaintenanceAuthority MaintenanceAuthority { get; set; }
    public string Morphology { get; set; }
    public string Status { get; set; }
    public RoadSegmentSideAttributes LeftSide { get; set; }
    public RoadSegmentSideAttributes RightSide { get; set; }
    public RoadSegmentLaneAttributes[] Lanes { get; set; }
    public RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    public RoadSegmentWidthAttributes[] Widths { get; set; }
    public RoadSegmentEuropeanRoad[] EuropeanRoads { get; set; }
    public RoadSegmentNationalRoad[] NationalRoads { get; set; }
    public RoadSegmentNumberedRoad[] NumberedRoads { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
}

public class RoadSegmentEuropeanRoad: IHaveHashFields
{
    public int AttributeId { get; set; }
    public string Number { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
}

public class RoadSegmentNationalRoad: IHaveHashFields
{
    public int AttributeId { get; set; }
    public string Number { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
}

public class RoadSegmentNumberedRoad: IHaveHashFields
{
    public int AttributeId { get; set; }
    public string Number { get; set; }
    public string Direction { get; set; }
    public int Ordinal { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
}
