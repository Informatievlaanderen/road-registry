namespace RoadRegistry.Projections.Tests.Projections.Pbs;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RoadRegistry.Pbs.Schema;
using RoadRegistry.Pbs.Schema.Records;

// Pins which PbsContext tables a projection rebuild wipes. The rebuild endpoint (ProjectionsController's
// TruncatePbsReadModel) truncates every table in the model except the projection-state row and the
// IEnumBasedCodeListRecord code lists, then replays the event stream — so a table is safe to truncate only if the
// replay restores it. Adding a table to PbsContext fails these tests until it is consciously categorised below:
// either it is projection output (truncated, replay restores it) or it must carry the IEnumBasedCodeListRecord
// marker (preserved, synced by PbsCodeListSyncService). Without this pin, a table populated outside the event
// replay would be silently wiped by the next rebuild.
public class PbsRebuildTableCoverageTests
{
    // Tables written by the event replay: the rebuild truncates them and RoadNetworkChangesPbsProjection restores
    // them from the event stream. Note that the Wegbeheerder code list belongs here: it is fed by organization
    // events, not by the enum sync, so it deliberately lacks the IEnumBasedCodeListRecord marker.
    private static readonly Type[] TruncatedOnRebuild =
    [
        // Features
        typeof(RoadSegmentRecord), typeof(DerivedRoadSegmentRecord), typeof(RoadNodeRecord),
        typeof(GradeJunctionRecord), typeof(GradeSeparatedJunctionRecord),
        typeof(EuropeanRoadRecord), typeof(NationalRoadRecord),
        // Dynamic attributes of a road segment
        typeof(RoadSegmentMorphologyAttributeRecord), typeof(RoadSegmentStreetNameAttributeRecord),
        typeof(RoadSegmentAccessRestrictionAttributeRecord), typeof(RoadSegmentCarTrafficDirectionAttributeRecord),
        typeof(RoadSegmentBikeTrafficDirectionAttributeRecord), typeof(RoadSegmentPedestrianTrafficDirectionAttributeRecord),
        typeof(RoadSegmentMaintenanceAuthorityAttributeRecord), typeof(RoadSegmentCategoryAttributeRecord),
        typeof(RoadSegmentSurfaceTypeAttributeRecord),
        // Event-driven code list (Wegbeheerder, from the organization projection)
        typeof(RoadSegmentMaintenanceAuthorityCodeListRecord),
        // Internal caches, fed by streetname/organization events
        typeof(StreetNameCacheRecord), typeof(OrganizationCacheRecord)
    ];

    // Tables the replay cannot restore: the rebuild must leave them alone. The enum-based code lists are synced by
    // PbsCodeListSyncService instead of by events; the projection-state row is not truncated but deleted by name.
    private static readonly Type[] PreservedOnRebuild =
    [
        typeof(RoadNodeTypeCodeListRecord), typeof(GradeSeparatedJunctionTypeCodeListRecord),
        typeof(RoadSegmentSideCodeListRecord), typeof(RoadSegmentMethodCodeListRecord),
        typeof(RoadSegmentMorphologyCodeListRecord), typeof(RoadSegmentDirectionCodeListRecord),
        typeof(RoadSegmentStatusCodeListRecord), typeof(RoadSegmentAccessRestrictionCodeListRecord),
        typeof(RoadSegmentSurfaceTypeCodeListRecord), typeof(RoadSegmentCategoryCodeListRecord),
        typeof(ProjectionStateItem)
    ];

    [Fact]
    public void EveryTableIsConsciouslyCategorised()
    {
        using var context = CreateContext();

        var categorised = TruncatedOnRebuild.Concat(PreservedOnRebuild).ToArray();
        categorised.Should().OnlyHaveUniqueItems("a table is either truncated or preserved, never both");

        var modelEntityTypes = context.Model.GetEntityTypes().Select(x => x.ClrType).ToArray();
        modelEntityTypes.Should().BeEquivalentTo(categorised,
            "every table in the PbsContext model must be consciously categorised as truncated-and-replayed or preserved on rebuild");
    }

    [Fact]
    public void RebuildTruncatesExactlyTheReplayableTables()
    {
        using var context = CreateContext();

        // The same rule TruncatePbsReadModel applies: everything except the projection-state row and the
        // enum-based code lists.
        var truncated = context.Model.GetEntityTypes()
            .Select(x => x.ClrType)
            .Where(x => x != typeof(ProjectionStateItem) && !typeof(IEnumBasedCodeListRecord).IsAssignableFrom(x))
            .ToArray();

        truncated.Should().BeEquivalentTo(TruncatedOnRebuild,
            "the rebuild must truncate exactly the tables the event replay restores; a preserved table missing the IEnumBasedCodeListRecord marker would be wiped without a way to restore it");
    }

    private static PbsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PbsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            // This test builds its own options, so across a test run EF creates more than its 20 internal
            // service providers; that is expected here and not a real leak, so silence the warning.
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .Options;
        return new PbsContext(options);
    }
}
