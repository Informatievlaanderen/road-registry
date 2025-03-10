namespace RoadRegistry.BackOffice.Messages;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentsRemoved : IHaveHash
{
    public const string EventName = "RoadSegmentsRemoved";

    public string GeometryDrawMethod { get; set; }
    public int[] RemovedRoadSegmentIds { get; set; }
    public int[] RemovedRoadNodeIds { get; set; }
    public RoadNodeTypeChanged[] ChangedRoadNodes { get; set; }
    public int[] RemovedGradeSeparatedJunctionIds { get; set; }

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
