namespace RoadRegistry.GradeSeparatedJunction.Events;

using System.Collections.Generic;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class GradeSeparatedJunctionRemoved : IHaveHash, ICreatedEvent
{
    public const string EventName = "GradeSeparatedJunctionRemoved";

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
