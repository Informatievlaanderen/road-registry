namespace RoadRegistry;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public interface IMartenEvent
{
    ProvenanceData Provenance { get; }
}
