namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentAccessRestrictionModified : IHaveHash
{
    public const string EventName = "RoadSegmentAccessRestrictionModified";

    public int Id { get; set; }
    public string CurrentValue { get; set; }
    public string PreviousValue { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
