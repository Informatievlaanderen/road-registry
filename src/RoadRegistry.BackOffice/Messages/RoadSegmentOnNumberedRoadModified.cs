namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentOnNumberedRoadModified: IHaveHash
{
    public const string EventName = "RoadSegmentOnNumberedRoadModified";

    public int AttributeId { get; set; }
    public string Direction { get; set; }
    public string Number { get; set; }
    public int Ordinal { get; set; }
    public int SegmentId { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
