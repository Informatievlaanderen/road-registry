namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class GradeSeparatedJunctionRemoved : IMessage, IHaveHash
{
    public const string EventName = "GradeSeparatedJunctionRemoved";

    public int Id { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
