namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Linq;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems Migrate(MigrateGradeSeparatedJunctionChange change, Provenance provenance)
    {
        var problems = Problems.WithContext(GradeSeparatedJunctionId);

        Apply(new GradeSeparatedJunctionWasMigrated
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            LowerRoadSegmentId = change.LowerRoadSegmentId,
            UpperRoadSegmentId = change.UpperRoadSegmentId,
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }

    // Projections may start from GradeSeparatedJunctionWasMigrated, so the geometry has to follow it, also when the
    // recompute after the change did not move the point and therefore raised nothing. A junction without geometry has
    // nothing to record.
    public void EnsureGeometryFollowsMigration(Provenance provenance)
    {
        if (IsRemoved || Geometry is null)
        {
            return;
        }

        var events = UncommittedEvents.ToList();
        var migratedIndex = events.FindLastIndex(x => x is GradeSeparatedJunctionWasMigrated);
        if (migratedIndex < 0 || events.Skip(migratedIndex + 1).Any(x => x is GradeSeparatedJunctionGeometryWasChanged))
        {
            return;
        }

        Apply(new GradeSeparatedJunctionGeometryWasChanged
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            Geometry = Geometry,
            Provenance = new ProvenanceData(provenance)
        });
    }
}
