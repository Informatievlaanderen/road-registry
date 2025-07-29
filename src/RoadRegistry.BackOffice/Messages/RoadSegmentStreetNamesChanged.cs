namespace RoadRegistry.BackOffice.Messages;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

[EventName(EventName)]
[EventDescription("Road segment streetnames were changed.")]
public class RoadSegmentsStreetNamesChanged : IMessage, IHaveHash, IWhen
{
    public const string EventName = "RoadSegmentsStreetNamesChanged";

    public string Reason { get; set; }
    public RoadSegmentStreetNamesChanged[] RoadSegments { get; set; }
    public string When { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}

public class RoadSegmentStreetNamesChanged
{
    public string GeometryDrawMethod { get; set; }
    public int Id { get; set; }
    public int Version { get; set; }
    public int? LeftSideStreetNameId { get; set; }
    public int? RightSideStreetNameId { get; set; }
}
