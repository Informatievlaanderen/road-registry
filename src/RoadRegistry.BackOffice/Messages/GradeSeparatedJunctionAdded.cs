namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class GradeSeparatedJunctionAdded : IMessage, IHaveHash
{
    public const string EventName = "GradeSeparatedJunctionAdded";

    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public int TemporaryId { get; set; }
    public string Type { get; set; }
    public int UpperRoadSegmentId { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
