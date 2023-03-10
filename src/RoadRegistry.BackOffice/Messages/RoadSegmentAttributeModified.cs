namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public abstract class RoadSegmentAttributeModified : IHaveHash
{
    public int Id { get; set; }
    public string CurrentValue { get; set; }
    public string PreviousValue { get; set; }
    public abstract string EventName { get; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}