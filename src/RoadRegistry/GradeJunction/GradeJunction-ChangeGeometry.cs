namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.GradeJunction.Events.V2;

public partial class GradeJunction
{
    public void ChangeGeometry(JunctionGeometry geometry, Provenance provenance)
    {
        if (IsRemoved || Geometry == geometry)
        {
            return;
        }

        Apply(new GradeJunctionGeometryWasChanged
        {
            GradeJunctionId = GradeJunctionId,
            Geometry = geometry,
            Provenance = new ProvenanceData(provenance)
        });
    }
}
