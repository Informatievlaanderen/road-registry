namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentAddedToEuropeanRoad: IHaveHash
{
    public const string EventName = "RoadSegmentAddedToEuropeanRoad";

    public int AttributeId { get; set; }
    public string Number { get; set; }
    public string SegmentGeometryDrawMethod { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
    public int? SegmentVersion { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
