namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using NodaTime;

public class RoadRegistryProvenanceData : ProvenanceData
{
    public RoadRegistryProvenanceData(Modification modification = Modification.Unknown, string? operatorName = null) : base(new Provenance(
        SystemClock.Instance.GetCurrentInstant(),
        Application.RoadRegistry,
        new Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason(string.Empty),
        new Operator(operatorName ?? OperatorName.Unknown),
        modification,
        Organisation.Agiv
    ))
    {
    }
}
