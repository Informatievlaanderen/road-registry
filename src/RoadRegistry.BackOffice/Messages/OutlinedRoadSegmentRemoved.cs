namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class OutlinedRoadSegmentRemoved : IHaveHash
{
    public const string EventName = "OutlinedRoadSegmentRemoved";

    public int Id { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
