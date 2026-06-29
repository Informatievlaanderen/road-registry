namespace RoadRegistry;

using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public interface IMartenEvent : IHaveHash
{
    ProvenanceData Provenance { get; }
}
