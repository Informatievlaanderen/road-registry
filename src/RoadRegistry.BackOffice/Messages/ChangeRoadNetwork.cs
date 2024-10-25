namespace RoadRegistry.BackOffice.Messages;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Utilities;

public class ChangeRoadNetwork : IMessage, IHasCommandProvenance
{
    private static readonly Guid Namespace = new("f5693e71-8a47-4fbd-b97c-12ff63a24e64");

    public RequestedChange[] Changes { get; set; }
    public string Operator { get; set; }
    public string OrganizationId { get; set; }
    public string Reason { get; set; }
    public string RequestId { get; set; }
    public string ExtractRequestId { get; set; }
    public Guid? DownloadId { get; set; }
    public Guid? TicketId { get; set; }

    public Provenance Provenance { get; }

    public ChangeRoadNetwork()
    {
    }

    public ChangeRoadNetwork(Provenance provenance)
    {
        Provenance = provenance;
    }

    public Guid CreateCommandId()
    {
        return Deterministic.Create(Namespace, $"ChangeRoadNetwork-{ToString()}");
    }

    private IEnumerable<object> IdentityFields()
    {
        yield return RequestId;
        yield return ExtractRequestId;

        foreach (var field in Provenance.GetIdentityFields())
        {
            yield return field;
        }
    }

    public override string ToString()
    {
        return ToStringBuilder.ToString(IdentityFields());
    }
}
