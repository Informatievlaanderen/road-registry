namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadNodeModified: IHaveHash
{
    public const string EventName = "RoadNodeModified";

    public RoadNodeGeometry Geometry { get; set; }
    public int Id { get; set; }
    public string Type { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
