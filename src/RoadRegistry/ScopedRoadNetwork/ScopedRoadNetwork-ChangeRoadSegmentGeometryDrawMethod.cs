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
    // needed. The statuses are validated up front: the whole request is rejected when any of the segments is not
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
            // The draw method sits on the segment's attributes, so the change rides the generic road segment
            // modification with everything else left null and thereby untouched.
            problems += ModifyRoadSegment(new ModifyRoadSegmentChange
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(change.RoadSegmentId),
                GeometryDrawMethod = change.GeometryDrawMethod
            }, context);
        }

        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    // A segment that cannot be found is left to ModifyRoadSegment, which reports it as not found.
    private Problems ValidateGeometryDrawMethodRoadSegments(IReadOnlyCollection<ChangeRoadSegmentGeometryDrawMethodChange> changes)
    {
        var problems = Problems.None;

        foreach (var change in changes)
        {
            if (!_roadSegments.TryGetValue(change.RoadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
            {
                continue;
            }

            // Everything reported below is about this one road segment, so it is collected under its context: every
            // error then identifies the segment the same way, whatever it is about.
            var roadSegmentProblems = Problems.WithContext(change.RoadSegmentId);

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
