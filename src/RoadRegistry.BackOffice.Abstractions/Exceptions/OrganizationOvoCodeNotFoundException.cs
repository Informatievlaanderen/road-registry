namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System;
using System.Runtime.Serialization;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice.Exceptions;

[Serializable]
public class OrganizationOvoCodeNotFoundException : RoadRegistryException
{
    public Guid TicketId { get; }
    public Provenance Provenance { get; }

    public OrganizationOvoCodeNotFoundException(Guid ticketId, Provenance provenance) : this("Could not find OVO code inside the known organizations", ticketId, provenance)
    {
    }

    public OrganizationOvoCodeNotFoundException(string message, Guid ticketId, Provenance provenance) : base(message)
    {
        TicketId = ticketId;
        Provenance = provenance;
    }

    protected OrganizationOvoCodeNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
