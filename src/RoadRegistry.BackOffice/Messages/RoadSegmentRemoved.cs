namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentRemoved: IHaveHash
{
    public const string EventName = "RoadSegmentRemoved";

    public int Id { get; set; }
    public string GeometryDrawMethod { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
