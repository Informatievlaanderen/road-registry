namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // VAL-6: the geometry draw method may only be changed on a road segment with one of these statuses.
    private static readonly RoadSegmentStatusV2[] ChangeGeometryDrawMethodAllowedStatuses =
    [
        RoadSegmentStatusV2.Gepland,
        RoadSegmentStatusV2.Gerealiseerd,
        RoadSegmentStatusV2.BuitenGebruik
    ];

    // 'Wijzig geometriemethode': only the draw method of the named segments changes. The geometry itself is never
    // touched, so the topology stays what it is: no spatial indexes to rebuild and no network-wide verification
    // needed. Everything is validated up front: the whole request is rejected when any of the segments is not
    // editable, so a request never lands half-applied.
    public RoadNetworkChangeResult ChangeRoadSegmentGeometryDrawMethod(
        IReadOnlyCollection<ChangeRoadSegmentGeometryDrawMethodChange> changes,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        var problems = ValidateGeometryDrawMethodRoadSegments(changes);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
        }

        foreach (var change in changes)
        {
            // Validated above, but the lookup stands on its own: an identifier that slipped through is still a
            // not-found, never a crash.
            if (!_roadSegments.TryGetValue(change.RoadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
            {
                problems += Problems.WithContext(change.RoadSegmentId) + new RoadSegmentNotFound();
                continue;
            }

            roadSegment.ChangeGeometryDrawMethod(change.GeometryDrawMethod, context);

            // A segment already carrying the desired draw method records nothing, and only what actually changed
            // belongs in the summary.
            if (roadSegment.GetRecordedChanges().Count > 0)
            {
                context.Summary.RoadSegments.Modified.Add(roadSegment.RoadSegmentId);
            }
        }

        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    private Problems ValidateGeometryDrawMethodRoadSegments(IReadOnlyCollection<ChangeRoadSegmentGeometryDrawMethodChange> changes)
    {
        var problems = Problems.None;

        foreach (var change in changes)
        {
            // Everything reported below is about this one road segment, so it is collected under its context: every
            // error then identifies the segment the same way, whatever it is about.
            var roadSegmentProblems = Problems.WithContext(change.RoadSegmentId);

            // VAL-4, VAL-5
            if (!_roadSegments.TryGetValue(change.RoadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
            {
                problems += roadSegmentProblems + new RoadSegmentNotFound();
                continue;
            }

            // A segment that has not completed its inwinning carries no attributes to change the draw method on.
            if (!roadSegment.HasMigrated())
            {
                problems += roadSegmentProblems + new RoadSegmentNotCompletedInwinning();
                continue;
            }

            // VAL-6
            if (!ChangeGeometryDrawMethodAllowedStatuses.Contains(roadSegment.Status))
            {
                problems += roadSegmentProblems + new RoadSegmentChangeGeometryDrawMethodStatusNotValid();
            }
        }

        return problems;
    }
}
