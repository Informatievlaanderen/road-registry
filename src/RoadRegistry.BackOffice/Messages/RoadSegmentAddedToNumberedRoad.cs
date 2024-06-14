namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentAddedToNumberedRoad: IHaveHash
{
    public const string EventName = "RoadSegmentAddedToNumberedRoad";

    public int AttributeId { get; set; }
    public string Direction { get; set; }
    public string Number { get; set; }
    public int Ordinal { get; set; }
    public string SegmentGeometryDrawMethod { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
    public int? SegmentVersion { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
