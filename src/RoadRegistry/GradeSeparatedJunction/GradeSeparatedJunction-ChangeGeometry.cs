namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;

public partial class GradeSeparatedJunction
{
    // Recompute-driven geometry update: emits GradeSeparatedJunctionGeometryWasChanged only when the point actually
    // changed (value equality on SRID+WKT) and is not null. Called from ScopedRoadNetwork.VerifyAndUpdateJunctions after
    // linked segments change.
    public void ChangeGeometry(JunctionGeometry geometry, Provenance provenance)
    {
        if (IsRemoved || Geometry == geometry)
        {
            return;
        }

        Apply(new GradeSeparatedJunctionGeometryWasChanged
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            Geometry = geometry,
            Provenance = new ProvenanceData(provenance)
        });
    }
}
