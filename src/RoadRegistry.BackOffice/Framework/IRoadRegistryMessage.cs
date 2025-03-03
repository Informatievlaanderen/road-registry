namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public interface IRoadRegistryMessage
{
    Guid MessageId { get; }
    ProvenanceData ProvenanceData { get; }
}
