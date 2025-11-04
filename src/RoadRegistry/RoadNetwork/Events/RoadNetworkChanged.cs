namespace RoadRegistry.RoadNetwork.Events;

using System.Collections.Generic;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadNetworkChanged: IHaveHash
{
    public const string EventName = "RoadNetworkChanged";

    public required string CausationId { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
