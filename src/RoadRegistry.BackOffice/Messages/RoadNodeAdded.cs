namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadNodeAdded: IHaveHash
{
    public const string EventName = "RoadNodeAdded";

    public RoadNodeGeometry Geometry { get; set; }
    public int Id { get; set; }
    public int Version { get; set; }
    public int TemporaryId { get; set; }
    public string Type { get; set; }
    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
