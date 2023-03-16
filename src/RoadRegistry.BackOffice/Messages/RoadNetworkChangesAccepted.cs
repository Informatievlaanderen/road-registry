namespace RoadRegistry.BackOffice.Messages;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using BackOffice;

[EventName(EventName)]
[EventDescription("Indicates the road network changes were accepted.")]
public class RoadNetworkChangesAccepted : IMessage, IHaveHash
{
    public const string EventName = "RoadNetworkChangesAccepted";
    
    public int Version { get; set; }
    public AcceptedChange[] Changes { get; set; }
    public string Operator { get; set; }
    public string Organization { get; set; }
    public string OrganizationId { get; set; }
    public string Reason { get; set; }
    public string RequestId { get; set; }
    public int TransactionId { get; set; }
    public string When { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
