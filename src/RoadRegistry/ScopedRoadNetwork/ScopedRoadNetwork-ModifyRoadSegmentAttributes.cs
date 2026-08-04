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
    // VAL-35: attribute values may only be changed on a road segment with one of these statuses.
    private static readonly RoadSegmentStatusV2[] ModifyAttributesAllowedStatuses =
    [
        RoadSegmentStatusV2.Gepland,
        RoadSegmentStatusV2.Gerealiseerd,
        RoadSegmentStatusV2.BuitenGebruik
    ];

    // 'Wijzig attribuutwaarden': a lighter form of Change that only modifies road segment attributes. The geometry is
    // never touched, so the spatial indexes do not have to be rebuilt and the network-wide verification of
    // AfterChangesApplied is not needed. The statuses are validated up front: the whole request is rejected when any of
    // the segments is not editable, so a request never lands half-applied.
    public RoadNetworkChangeResult ModifyRoadSegmentAttributes(
        IReadOnlyCollection<ModifyRoadSegmentChange> changes,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var idTranslator = new IdentifierTranslator();
        var context = new ScopedRoadNetworkChangeContext(this, idTranslator, provenance, logger);

        var problems = ValidateRoadSegments(changes);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
        }

        foreach (var change in changes)
        {
            problems += ModifyRoadSegment(change, context);
        }

        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    // A segment that cannot be found is left to ModifyRoadSegment, which reports it as not found.
    private Problems ValidateRoadSegments(IReadOnlyCollection<ModifyRoadSegmentChange> changes)
    {
        var problems = Problems.None;

        foreach (var change in changes)
        {
            var roadSegmentId = change.RoadSegmentIdReference.RoadSegmentId;

            if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
            {
                continue;
            }

            // A segment that has not completed its inwinning carries no dynamically segmented attributes to change.
            if (!roadSegment.HasMigrated())
            {
                problems += new RoadSegmentNotCompletedInwinning(roadSegmentId);
                continue;
            }

            // VAL-35
            if (!ModifyAttributesAllowedStatuses.Contains(roadSegment.Status))
            {
                problems += new RoadSegmentChangeAttributesStatusNotValid(roadSegmentId);
            }
        }

        return problems;
    }
}
